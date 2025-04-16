using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Smart_ERP.Modules.Auth.Models;

namespace Smart_ERP.Modules.Auth.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(User user)
        {
            // check env first
            // if not found, check appsettings.json
            var key = Environment.GetEnvironmentVariable("JWT_KEY") ?? _config["JwtSettings:Key"];
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _config["JwtSettings:Issuer"];
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _config["JwtSettings:Audience"];
            var expire = Environment.GetEnvironmentVariable("JWT_EXPIRE_MINUTES") ?? _config["JwtSettings:ExpireMinutes"];

            if (string.IsNullOrEmpty(key))
                throw new Exception("JWT key is missing");

            if (!double.TryParse(expire, out double expireMinutes))
                expireMinutes = 60;

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(ClaimTypes.Role, user.Role.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

