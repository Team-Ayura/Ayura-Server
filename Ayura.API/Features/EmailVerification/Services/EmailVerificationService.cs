using AutoMapper;
using Ayura.API.Configuration;
using Ayura.API.Features.EmailVerification.DTOs;
using Ayura.API.Features.EmailVerification.Helpers;
using Ayura.API.Features.Profile.Helpers.MailService;
using Ayura.API.Global.MailService.DTOs;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ayura.API.Features.EmailVerification.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IMongoCollection<Evc> _evcCollection;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;

    public EmailVerificationService(IAppSettings appSettings, IAyuraDatabaseSettings settings,
        IMongoClient mongoClient, IOptions<AppSettings> appSettingsOptions, IMailService mailService)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _evcCollection = database.GetCollection<Evc>(settings.EvcCollection);
        _appSettings = appSettingsOptions;
        _mailService = mailService;

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EvcRequestDto, Evc>().ReverseMap();
            cfg.CreateMap<EvcVerifyDto, Evc>().ReverseMap();
        });
        

        _mapper = mapperConfig.CreateMapper();
    }

    public async Task<string> GenerateEmailVerificationCode(EvcRequestDto evcRequestDto, string userId)
    {
        var evc = EvcGenerator.GenerateEvc();
        var evcModel = _mapper.Map<Evc>(evcRequestDto);

        evcModel.EvcReq = evc;
        evcModel.UserId = userId;

        evcModel.ExpiryTime = DateTime.UtcNow.AddMinutes(10);

        // Console.Write("EVC Expiry Time: " + evcModel.ExpiryTime + "\n");


        var filter = Builders<Evc>.Filter.Eq("UserId", userId);

        // Console.Write("Filter Created\n");

        var evcFromDatabase = _evcCollection.Find(filter).FirstOrDefault();

        if (evcFromDatabase != null)
        {
            // Console.Write("EVC from database is not null\n");
            await _evcCollection.ReplaceOneAsync(filter, evcModel);
        }
        else
        {
            // Console.Write("EVC from database is null");
            await _evcCollection.InsertOneAsync(evcModel);
        }
        
        // Send Email
        var mailData = new MailData
        {
            EmailToId = evcRequestDto.Email,
            EmailToName = evcRequestDto.Email,
            EmailSubject = "Ayura Email Verification Code",
            EmailBody = "Your email verification code is: " + evc
        };
        
        await _mailService.SendMailAsync(mailData);
        
        return "EVC sent via email\n";
    }

    public Task<string> VerifyEmail(EvcVerifyDto evcVerifyDto, string userId)
    {
        var filter = Builders<Evc>.Filter.Eq("UserId", userId);
        var evcFromDatabase = _evcCollection.Find(filter).FirstOrDefault();

        if (evcFromDatabase == null) return Task.FromResult("No EVC found for this user");

        if (evcFromDatabase.EvcReq != evcVerifyDto.VerificationCode) return Task.FromResult("EVC does not match");

        if (evcFromDatabase.ExpiryTime < DateTime.UtcNow) return Task.FromResult("EVC has expired");

        return Task.FromResult("EVC verified");
    }
}