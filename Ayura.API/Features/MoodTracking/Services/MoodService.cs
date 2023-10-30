using AutoMapper;
using Ayura.API.Features.MoodTracking.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MongoDB.Driver;

namespace Ayura.API.Features.MoodTracking.Services;

public class MoodService : IMoodService
{
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMapper _mapper;

    public MoodService(IMongoClient client, IAyuraDatabaseSettings settings)
    {
        var database = client.GetDatabase("Ayura");
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        
        // DTO to Model
        
    }

    public async Task<String> AddMoodData(AddMoodRequestDTO addMoodRequestDto, String userId)
    {
        var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user == null)
        {
            return "User not found";
        }
        var mood = new Mood
        {
            Date = addMoodRequestDto.Date,
            Time = addMoodRequestDto.Time,
            MoodName = addMoodRequestDto.MoodName,
            MoodWeight = addMoodRequestDto.MoodWeight
        };
        user.MoodsHistories.Add(mood);
        await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);
        return "success";
    }
    

    
    
    

}