using System.Text;

namespace Starter_App.Models
{
    public class AuthRequest
    {
        private readonly string _issuer;
        private readonly string _assertionConsumerServiceUrl;

        public AuthRequest(string issuer, string assertionConsumerServiceUrl)
        {
            _issuer = issuer;
            _assertionConsumerServiceUrl = assertionConsumerServiceUrl;
        }

        public string GetRedirectUrl(string samlEndpoint)
        {
            var requestId = "_" + Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

            var samlRequest = $@"<samlp:AuthnRequest xmlns:samlp=""urn:oasis:names:tc:SAML:2.0:protocol""
                xmlns:saml=""urn:oasis:names:tc:SAML:2.0:assertion""
                ID=""{requestId}""
                Version=""2.0""
                IssueInstant=""{timestamp}""
                Destination=""{samlEndpoint}""
                AssertionConsumerServiceURL=""{_assertionConsumerServiceUrl}"">
                <saml:Issuer>{_issuer}</saml:Issuer>
                <samlp:NameIDPolicy Format=""urn:oasis:names:tc:SAML:1.1:nameid-format:unspecified"" AllowCreate=""true""/>
            </samlp:AuthnRequest>";

            var encodedRequest = Convert.ToBase64String(Encoding.UTF8.GetBytes(samlRequest));
            var encodedRequestUrl = Uri.EscapeDataString(encodedRequest);

            return $"{samlEndpoint}?SAMLRequest={encodedRequestUrl}";
        }
    }
}
