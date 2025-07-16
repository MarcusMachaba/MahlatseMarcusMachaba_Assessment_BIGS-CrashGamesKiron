using System;

namespace Core.ApplicationModels.KironTestAPI
{
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiresAt { get; set; } 
    }
}
