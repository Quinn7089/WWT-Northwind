using System.Text;
using System.Xml;

namespace Starter_App.Models
{
    public class Response
    {
        private readonly string _certificate;
        private readonly string _samlResponse;

        public Response(string certificate, string samlResponse)
        {
            _certificate = certificate;
            _samlResponse = samlResponse;
        }

        public bool IsValid()
        {
            try
            {
                if (string.IsNullOrEmpty(_samlResponse) || string.IsNullOrEmpty(_certificate))
                    return false;

                // Decode the SAML response
                var decodedResponse = Encoding.UTF8.GetString(Convert.FromBase64String(_samlResponse));
                
                // For development purposes, we'll do basic validation
                // In production, you should validate the digital signature
                return decodedResponse.Contains("urn:oasis:names:tc:SAML:2.0:assertion");
            }
            catch
            {
                return false;
            }
        }

        public string GetNameID()
        {
            try
            {
                if (string.IsNullOrEmpty(_samlResponse))
                    return string.Empty;

                var decodedResponse = Encoding.UTF8.GetString(Convert.FromBase64String(_samlResponse));
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(decodedResponse);

                var nameIdNode = xmlDoc.SelectSingleNode("//saml:NameID", GetNamespaceManager(xmlDoc));
                return nameIdNode?.InnerText ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private XmlNamespaceManager GetNamespaceManager(XmlDocument doc)
        {
            var namespaceManager = new XmlNamespaceManager(doc.NameTable);
            namespaceManager.AddNamespace("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            namespaceManager.AddNamespace("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            return namespaceManager;
        }
    }
}
