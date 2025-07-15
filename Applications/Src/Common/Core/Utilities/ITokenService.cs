using Core.ApplicationModels.KironTestAPI;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Core.Utilities
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateAccessToken(User user);
    }

    public class JwtService : ITokenService
    {
        private readonly Logger.Logger mLog;
        private readonly JwtSettings _settings;

        public JwtService(IOptions<JwtSettings> settings)
        {
            mLog = Logger.Logger.GetLogger(typeof(JwtService));
            _settings = settings.Value;
        }

        public (string Token, DateTime ExpiresAt) GenerateAccessToken(User user)
        {
            mLog.Debug($"Token request GenerateToken() invoke for: {user.UserName}");
            // Important that this stays UTC
            var now = DateTime.UtcNow; 

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var expires = now.Add(_settings.AccessTokenLifetime);
            var base64Key = _settings.SecretKey!;
            var keyBytes = Convert.FromBase64String(base64Key);
            //var key = Encoding.ASCII.GetBytes(_settings.SecretKey!);//Encoding.UTF8.GetBytes(_settings.SecretKey!);
            var creds = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                notBefore: now,
                expires: expires,//DateTime.UtcNow.AddMinutes(double.Parse(_settings.AccessTokenLifetime!)),
                signingCredentials: creds);
            mLog.Debug($"Token request GenerateToken() invoke for: {user.UserName} succeeded");
            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }
    }
}
