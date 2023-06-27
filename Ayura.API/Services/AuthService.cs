using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Ayura.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IMongoCollection<User> _userCollection;
        private readonly IOptions<AppSettings> _appSettings; // Add this field

        public AuthService(IAppSettings appSettings, IAyuraDatabaseSettings settings, IMongoClient mongoClient, IOptions<AppSettings> appSettingsOptions)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _userCollection = database.GetCollection<User>(settings.UserCollection);
            _appSettings = appSettingsOptions; // Assign the appSettingsOptions to the field
        }

        public async Task<string> AuthenticateUser(string email, string password)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Email, email);
            var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

            if (user == null)
            {
                return null; // no user with the given email
            }
            else if (!VerifyPassword(password, user.Password)) // Fixed the condition
            {
                return null; // password does not match
            }

            // Generate and return a JWT token to authenticated user
            var token = GenerateJwtToken(user.Email);
            return token;
        }

        private bool VerifyPassword(string password, string hashedPassword)
        {
            // USING BCRYPT
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
            // USING ARGON2
            // return Argon2.Verify(hashedPassword, password);
        }

        private string GenerateJwtToken(string email)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Value.SecretKey); // Use _appSettings to access the secret key
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("email", email) }),
                // Expires = DateTime.UtcNow.AddMinutes(30), // expires after 30 minutes
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
