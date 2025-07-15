using System;
using System.Collections.Generic;
using System.Text;

namespace Core.ApplicationModels.KironTestAPI
{
    public class JwtSettings
    {
        public string SecretKey { get; set; }
        /// <summary>
        /// The expiration time for the generated tokens.
        /// </summary>
        /// <remarks>The default is 1 hour.</remarks>
        public TimeSpan AccessTokenLifetime { get; set; } = TimeSpan.FromMinutes(60);
        /// <summary>
        /// The expiration time for the generated tokens.
        /// </summary>
        /// <remarks>The default is 1 hour.</remarks>
        public TimeSpan RefreshTokenLifetime { get; set; } = TimeSpan.FromMinutes(60);
        public string Issuer { get; set; }
        /// <summary>
        /// The relative request path to listen on.
        /// </summary>
        /// <remarks>The default path is <c>/token</c>.</remarks>
        public string TokenPath { get; set; } = "api/token";
        /// <summary>
        /// The Audience (aud) claim for the generated tokens.
        /// </summary>
        public string Audience { get; set; }
        public TimeSpan ClockSkew { get; set; }
    }
}
