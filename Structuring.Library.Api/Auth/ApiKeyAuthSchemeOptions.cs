using Microsoft.AspNetCore.Authentication;

namespace Structuring.Library.Api.Auth
{
    public class ApiKeyAuthSchemeOptions : AuthenticationSchemeOptions
    {
        // Realistically this value should come from Azure Key Vault or AWS Secret Manager sort of services
        public string ApiKey { get; set; } = "VerySecret";
    }
}
