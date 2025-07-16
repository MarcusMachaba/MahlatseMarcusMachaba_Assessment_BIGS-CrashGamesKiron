using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Core.Utilities
{
    public class Utility
    {
        public static string SecretAuthenticationKey { get; private set; }
        public static void SetJWTSecretAuthenticationKey(string secretAuthenticationKey)
        {
            SecretAuthenticationKey = secretAuthenticationKey;
        }

        public static (string, DateTime) GenerateJwtToken(string userName, string role)
        {

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretAuthenticationKey);
            var expires = DateTime.Now.AddMinutes(60);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // TODO: Pull these from a configuration / appSettin
                Issuer = "",
                Audience = "",
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, userName),
                    new Claim(ClaimTypes.Role, role)
                }),

                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (tokenHandler.WriteToken(token), expires);
        }

        
    }
}
