using AutoMapper;
using Ayura.API.Features.EmailVerification.DTOs;
using Ayura.API.Features.EmailVerification.Helpers;
using Ayura.API.Features.EmailVerification.Services;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IMongoCollection<EVC> _evcCollection;
    private readonly IMapper _mapper;

    public EmailVerificationService(IAppSettings appSettings, IAyuraDatabaseSettings settings,
        IMongoClient mongoClient, IOptions<AppSettings> appSettingsOptions)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _evcCollection = database.GetCollection<EVC>(settings.EvcCollection);
        _appSettings = appSettingsOptions;

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EVCRequestDTO, EVC>().ReverseMap();
            cfg.CreateMap<EVCVerifyDTO, EVC>().ReverseMap();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    public async Task<string> GenerateEmailVerificationCode(EVCRequestDTO evcRequestDto, string userId)
    {
        var evc = EVCGenerator.GenerateEVC();
        var evcModel = _mapper.Map<EVC>(evcRequestDto);

        evcModel.Evc = evc;
        evcModel.UserId = userId;

        evcModel.ExpiryTime = DateTime.UtcNow.AddMinutes(10);

        Console.Write("EVC Expiry Time: " + evcModel.ExpiryTime + "\n");


        var filter = Builders<EVC>.Filter.Eq("UserId", userId);

        Console.Write("Filter Created");

        var evcFromDatabase = _evcCollection.Find(filter).FirstOrDefault();

        if (evcFromDatabase != null)
        {
            Console.Write("EVC from database is not null");
            _evcCollection.ReplaceOneAsync(filter, evcModel);
        }
        else
        {
            Console.Write("EVC from database is null");
            _evcCollection.InsertOneAsync(evcModel);
        }

        return "EVC sent to email and stored in the DB";
    }

    public Task<string> VerifyEmail(EVCVerifyDTO evcVerifyDto, string userId)
    {
        var filter = Builders<EVC>.Filter.Eq("UserId", userId);
        var evcFromDatabase = _evcCollection.Find(filter).FirstOrDefault();

        if (evcFromDatabase == null) return Task.FromResult("No EVC found for this user");

        if (evcFromDatabase.Evc != evcVerifyDto.verificationcode) return Task.FromResult("EVC does not match");

        if (evcFromDatabase.ExpiryTime < DateTime.UtcNow) return Task.FromResult("EVC has expired");

        return Task.FromResult("EVC verified");
    }
}