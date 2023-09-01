using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Ayura.API.Configuration;
using Ayura.API.Features.Registration.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Ayura.API.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class AuthService : IAuthService
{
    private readonly IOptions<AppSettings> _appSettings; // Add this field
    private readonly IMapper _mapper;
    private readonly IPasswordHasher<string> _passwordHasher;
    private readonly IMongoCollection<User> _userCollection;

    public AuthService(IAppSettings appSettings, IAyuraDatabaseSettings settings, IMongoClient mongoClient,
        IOptions<AppSettings> appSettingsOptions, IPasswordHasher<string> passwordHasher)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        _appSettings = appSettingsOptions; // Assign the appSettingsOptions to the field
        _passwordHasher = passwordHasher; // to hash the password 

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<SignupRequest, User>(); });

        _mapper = mapperConfig.CreateMapper();
    }

    public async Task<LoginResponse> AuthenticateUser(string email, string password)
    {
        var filter = Builders<User>.Filter.Eq(u => u.Email, email);
        var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

        if (user == null)
            throw new Exception("Invalid email"); // no user with the given email
        if (!VerifyPassword(password, user.Password)) // Fixed the condition
            throw new Exception("Invalid password"); // password does not match

        // Generate and return a JWT token to authenticated user
        var response = new LoginResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            ProfileImage = "",
            Token = GenerateJwtToken(user)
        };
        return response;
    }

    public async Task<User> RegisterUser(SignupRequest signupRequest)
    {
        // check whether the email already exist
        var filter = Builders<User>.Filter.Eq(u => u.Email, signupRequest.Email);
        var user = await _userCollection.Find(filter).FirstOrDefaultAsync();
        if (user != null) throw new Exception("Email exists");

        // Hash the password
        var hashedPassword = _passwordHasher.HashPassword(null, signupRequest.Password);
        signupRequest.Password = hashedPassword;

        // map signin request to a user
        user = _mapper.Map<User>(signupRequest);
        user.Role = "user";

        // create the user in mongo
        _userCollection.InsertOne(user);

        return user;
    }

    // private helper methods
    /*private bool VerifyPassword(string password, string hashedPassword)
    {
        // Verify the password using BCrypt
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }*/
    public bool VerifyPassword(string password, string hashedPassword)
    {
        var passwordVerificationResult = _passwordHasher.VerifyHashedPassword(null, hashedPassword, password);

        return passwordVerificationResult == PasswordVerificationResult.Success;
    }

    private string GenerateJwtToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_appSettings.Value.SecretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("iss", "https://localhost:7034"), // Add issuer claim
                new Claim("aud", "ayura-flutter-app")
            }),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}