using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Starter_App.src.backend.Models;
using Starter_App.src.backend.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography.X509Certificates;


namespace Starter_App.Controllers
{
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration _config;
        private readonly AppStarterContext _dbContext;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(IConfiguration config, AppStarterContext dbContext, UserManager<AspNetUser> userManager, ILogger<AuthenticationController> logger)
        {
            _config = config;
            _dbContext = dbContext;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        [Route("test")]
        public async Task<IActionResult> Test()
        {
            // Check both SAML and JWT authentication
            var samlAuth = await HttpContext.AuthenticateAsync("SAML");
            var jwtAuth = await HttpContext.AuthenticateAsync("JWT");
            
            var isAuthenticated = samlAuth.Succeeded || jwtAuth.Succeeded;
            var principal = samlAuth.Succeeded ? samlAuth.Principal : jwtAuth.Principal;
            
            return Ok(new { 
                message = "AuthenticationController is working!",
                isAuthenticated = isAuthenticated,
                userName = principal?.Identity?.Name,
                authType = principal?.Identity?.AuthenticationType,
                claims = principal?.Claims?.Select(c => new { c.Type, c.Value }).ToArray() ?? new object[0],
                samlAuthenticated = samlAuth.Succeeded,
                jwtAuthenticated = jwtAuth.Succeeded
            });
        }

        [HttpGet]
        [Route("loginWithSaml")]
        public IActionResult LoginWithSaml(bool forceLogin = false)
        {
            try
            {
                var samlEndpoint = _config["SamlEndpoint"];
                var issuer = _config["SsoIssuerApplicationName"];
                var assertionUrl = _config["SamlAssertionURL"];

                var request = new Saml.AuthRequest(issuer, assertionUrl);
                var redirectUrl = request.GetRedirectUrl(samlEndpoint);
                
                // If this is a forced login (after logout), add prompt=login to force re-authentication
                if (forceLogin)
                {
                    var separator = redirectUrl.Contains("?") ? "&" : "?";
                    redirectUrl += $"{separator}prompt=login";
                }
                
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"SAML Login Error: {ex.Message}");
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("consumeSaml")]
        public async Task<IActionResult> ConsumeSaml()
        {
            try
            {
                string baseUrl = _config["BaseURL"] ?? _config["DefaultBaseURL"];
                
                if (!Request.Form.ContainsKey("SAMLResponse"))
                {
                    return BadRequest("No SAMLResponse found in form data");
                }
                
                var samlResponse = Request.Form["SAMLResponse"].ToString();
                string samlCert;
                bool certIsValid = true;
                
                try
                {
                    samlCert = BuildSamlCertificate();
                    certIsValid = IsSamlCertificateValid(samlCert);
                }
                catch (Exception)
                {
                    certIsValid = false;
                    samlCert = string.Empty;
                }

                bool samlResponseValid = false;
                string username = string.Empty;
                
                if (certIsValid && !string.IsNullOrEmpty(samlCert))
                {
                    try
                    {
                        var response = new Saml.Response(samlCert, samlResponse);
                        samlResponseValid = response.IsValid();
                        
                        if (samlResponseValid)
                        {
                            username = response.GetNameID();
                        }
                    }
                    catch (Exception)
                    {
                        samlResponseValid = false;
                    }
                }

                if (!certIsValid || !samlResponseValid || string.IsNullOrEmpty(username))
                {
                    if (string.IsNullOrEmpty(username))
                    {
                        try
                        {
                            var decodedResponse = Encoding.UTF8.GetString(Convert.FromBase64String(samlResponse));
                            var xmlDoc = new System.Xml.XmlDocument();
                            xmlDoc.LoadXml(decodedResponse);
                            var nameIdNode = xmlDoc.SelectSingleNode("//*[local-name()='NameID']");
                            username = nameIdNode?.InnerText ?? "unknown";
                        }
                        catch
                        {
                            username = "unknown";
                        }
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.NameIdentifier, "temp-" + username),
                        new Claim("IsTempUser", "true")
                    };
                    
                    if (!certIsValid)
                    {
                        claims.Add(new Claim("CertificateError", "true"));
                    }
                    
                    var claimsIdentity = new ClaimsIdentity(claims, "SAML");
                    await HttpContext.SignInAsync("SAML", new ClaimsPrincipal(claimsIdentity));
                    
                    var tempUserRedirectUrl = _config["LoginRedirectUrls:TempUser"] ?? "/";
                    return Redirect($"{baseUrl}{tempUserRedirectUrl}");
                }

                var user = await _userManager.FindByNameAsync(username);
                
                if (user == null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, username),
                        new Claim(ClaimTypes.NameIdentifier, "temp-" + username),
                        new Claim("IsTempUser", "true")
                    };
                    
                    var claimsIdentity = new ClaimsIdentity(claims, "SAML");
                    await HttpContext.SignInAsync("SAML", new ClaimsPrincipal(claimsIdentity));
                    
                    var tempUserRedirectUrl = _config["LoginRedirectUrls:TempUser"] ?? "/";
                    return Redirect($"{baseUrl}{tempUserRedirectUrl}");
                }
                
                var token = await BuildToken(user);
                var option = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(10)
                };
                Response.Cookies.Append("jwt", token, option);
                
                var userRedirectUrl = _config["LoginRedirectUrls:AuthenticatedUser"] ?? "/user";
                return Redirect($"{baseUrl}{userRedirectUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing SAML authentication response");
                return BadRequest($"SAML Consume Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Sign out from SAML authentication
                await HttpContext.SignOutAsync("SAML");
                
                // Clear any additional cookies
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }
                
                // Get logout endpoint from configuration
                var logoutEndpoint = _config["SamlLogoutEndpoint"];
                var baseUrl = _config["BaseURL"] ?? _config["DefaultBaseURL"];
                var loginRedirectUrl = _config["LogoutRedirectUrls:LoginWithSaml"] ?? "/Authentication/loginWithSaml?forceLogin=true";
                
                if (!string.IsNullOrEmpty(logoutEndpoint))
                {
                    // Build Microsoft Azure AD logout URL that clears the session and redirects to forced login
                    var logoutUrl = $"{logoutEndpoint}?post_logout_redirect_uri={Uri.EscapeDataString($"{baseUrl}{loginRedirectUrl}")}";
                    
                    return Redirect(logoutUrl);
                }
                else
                {
                    return Redirect(loginRedirectUrl);
                }
            }
            catch (Exception)
            {
                var loginRedirectUrl = _config["LogoutRedirectUrls:LoginWithSaml"] ?? "/Authentication/loginWithSaml?forceLogin=true";
                return Redirect(loginRedirectUrl);
            }
        }

        [HttpGet]
        [Route("logoutCallback")]
        public IActionResult LogoutCallback()
        {
            try
            {
                var homeRedirectUrl = _config["LogoutRedirectUrls:LogoutCallback"] ?? "/";
                return Redirect(homeRedirectUrl);
            }
            catch (Exception)
            {
                var homeRedirectUrl = _config["LogoutRedirectUrls:LogoutCallback"] ?? "/";
                return Redirect(homeRedirectUrl);
            }
        }
    
        private string BuildSamlCertificate()
        {
            var rawCert = _config["SamlCert"];

            if (string.IsNullOrWhiteSpace(rawCert))
            {
                throw new InvalidOperationException("SAML certificate configuration value is missing from appsettings.json. Please add the SamlCert configuration.");
            }

            try
            {
                rawCert = rawCert.Trim()
                                 .Replace("\r", string.Empty)
                                 .Replace("\n", string.Empty)
                                 .Replace("\\n", string.Empty);

                const int lineLength = 64;
                var builder = new StringBuilder();
                builder.AppendLine("-----BEGIN CERTIFICATE-----");

                for (var i = 0; i < rawCert.Length; i += lineLength)
                {
                    var length = Math.Min(lineLength, rawCert.Length - i);
                    builder.AppendLine(rawCert.Substring(i, length));
                }

                builder.AppendLine("-----END CERTIFICATE-----");

                return builder.ToString();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to build SAML certificate from configuration. The certificate format may be invalid.", ex);
            }
        }

        private bool IsSamlCertificateValid(string certificate)
        {
            try
            {
                var base64Cert = certificate
                    .Replace("-----BEGIN CERTIFICATE-----", "")
                    .Replace("-----END CERTIFICATE-----", "")
                    .Replace("\r", "")
                    .Replace("\n", "")
                    .Trim();
                
                var cert = new X509Certificate2(Convert.FromBase64String(base64Cert));

                var now = DateTime.UtcNow;
                if (now > cert.NotAfter || now < cert.NotBefore)
                {
                    _logger.LogError(
                        "SAML certificate is not valid. Valid from {NotBefore} to {NotAfter}, Current time: {Now}",
                        cert.NotBefore.ToUniversalTime(),
                        cert.NotAfter.ToUniversalTime(),
                        now
                    );
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> BuildToken(AspNetUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            // Query roles via EF
            try
            {
                var roles = await _dbContext.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(
                        _dbContext.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => r.Name
                    ).ToListAsync();

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                if (roles.Count == 0)
                {
                    claims.Add(new Claim(ClaimTypes.Role, Roles.User));
                    roles = new List<string> { Roles.User };
                }

                var userPermissions = await PermissionService.GetPermissionsForRolesAsync(roles, _dbContext);
                foreach (var permission in userPermissions)
                {
                    claims.Add(new Claim("Permission", permission));
                }

            }
            catch (Exception)
            {
                claims.Add(new Claim(ClaimTypes.Role, Roles.User));
                
                var defaultPermissions = await PermissionService.GetPermissionsForRoleAsync(Roles.User, _dbContext);
                foreach (var permission in defaultPermissions)
                {
                    claims.Add(new Claim("Permission", permission));
                }
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                null, // issuer
                null, // audience
                claims,
                expires: DateTime.Now.AddDays(Int16.Parse(_config["Jwt:ValidFor"])),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}