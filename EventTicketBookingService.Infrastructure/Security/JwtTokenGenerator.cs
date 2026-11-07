using EventTicketBookingService.Application.Interfaces;
using EventTicketBookingService.Domain;
using EventTicketBookingService.Domain.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace EventTicketBookingService.Infrastructure.Security
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _options;

        public JwtTokenGenerator(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public string GenerateToken(int userId, string login, UserRoles role)
        {
            var claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = userId.ToString(),
                [JwtRegisteredClaimNames.UniqueName] = login,
                [ClaimTypes.Role] = role.ToString(),
                [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
            };

            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_options.Secret!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                Claims = claims,
                NotBefore = DateTime.UtcNow,
                Expires = DateTime.UtcNow.AddMinutes(_options.LifeTimeMinutes),
                SigningCredentials = credentials
            };

            var tokenString = new JsonWebTokenHandler().CreateToken(descriptor);

            return tokenString;
        }
    }
}
