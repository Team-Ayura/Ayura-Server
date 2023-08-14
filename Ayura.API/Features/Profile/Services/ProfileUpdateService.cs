using AutoMapper;
using Ayura.API.Features.Profile.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ayura.API.Features.Profile.Services;

public class ProfileUpdateService : IProfileUpdateService
{
    private readonly IOptions<AppSettings> _appSettings; // Add this field
    private readonly IMapper _mapper;
    private readonly IMongoCollection<User> _userCollection;

    public ProfileUpdateService(IAppSettings appSettings, IAyuraDatabaseSettings settings, IMongoClient mongoClient,
        IOptions<AppSettings> appSettingsOptions)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        _appSettings = appSettingsOptions; // Assign the appSettingsOptions to the field

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<User, ProfileDetailsDTO>();
            cfg.CreateMap<UpdateDetailsDTO, User>();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    public async Task<ProfileDetailsDTO> UpdateProfileDetails(string id, UpdateDetailsDTO updateDetailsDto)
    {
        Console.Write("Function in the Service is called\n");
        Console.Write($"First Name: {updateDetailsDto.FirstName}\n");
        var filter = Builders<User>.Filter.Eq("Id", id);
        var user = await _userCollection.Find(filter).FirstOrDefaultAsync();

        if (user == null)
        {
            Console.Write("No User\n");
            // Handle the case where the user with the given email is not found
            return null;
        }

        Console.Write("User Exists\n");


        // Change the data in the database according to the UpdateDetailsDTO
        _mapper.Map(updateDetailsDto, user);

        Console.Write($"First Name Changed - {user.FirstName}\n");
        // Update the user in the database
        await _userCollection.ReplaceOneAsync(filter, user);

        // Map the User model to ProfileDetailsDTO
        var profileDetails = _mapper.Map<ProfileDetailsDTO>(user);

        /*return user;*/
        return profileDetails;
    }
}