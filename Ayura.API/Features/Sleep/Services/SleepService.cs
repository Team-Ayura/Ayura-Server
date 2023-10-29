using AutoMapper;
using Ayura.API.Features.Activity.DTOs;
using Ayura.API.Features.Activity.Models;
using Ayura.API.Features.Sleep.DTOs;
using Ayura.API.Features.Sleep.Models;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using MimeKit.Encodings;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Ayura.API.Features.Sleep.Services;

public class SleepService : ISleepService
{
    private readonly IMongoCollection<User> _userCollection;
    private IMapper _mapper;
    
    //constructor
    public SleepService(IAyuraDatabaseSettings settings, IMongoClient mongoClient)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        
        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg => { cfg.CreateMap<AddCyclingRequest, CyclingHistory>(); });

        _mapper = mapperConfig.CreateMapper();
    }
    
    // 1. Add sleep data each day 
    public async Task<string> AddSleepData(AddSleepDataDto addSleepDataDto)
    {
        
        // map signin request to a user
        var oneSleepData = new SleepHistory
        {
            Id = ObjectId.GenerateNewId().ToString(),
            BedTime = addSleepDataDto.BedTime,
            WakeupTime = addSleepDataDto.WakeupTime,
            Quality = addSleepDataDto.Quality,
            BeforeSleepAffect = addSleepDataDto.BeforeSleepAffect,
            AfterSleepAffect = addSleepDataDto.AfterSleepAffect,
            
        };
        
        var filter = Builders<User>.Filter.Eq(u => u.Id, addSleepDataDto.UserId);
        var update = Builders<User>.Update.Push<SleepHistory>(u => u.SleepHistories, oneSleepData);

        await _userCollection.UpdateOneAsync(filter, update);

        return "success";
    }

}