using Microsoft.IdentityModel.Tokens;
using RentVilla_API.Entities;
using RentVilla_API.Interfaces;
using RentVilla_API.Logger.LoogerInterfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RentVilla_API.Services
{
    public class TokenServices : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly ILoggerDev _loggerDev;
        public TokenServices(IConfiguration config, ILoggerDev loggerDev)
        {
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["TokenKey"]));
            _loggerDev = loggerDev;
            
        }
        public string CreateToken(AppUser user)
        {
            var claims = new List<Claim>
            {
                new Claim (JwtRegisteredClaimNames.Email, user.Email)
            };

            var credientials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays (10),
                SigningCredentials = credientials,
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
