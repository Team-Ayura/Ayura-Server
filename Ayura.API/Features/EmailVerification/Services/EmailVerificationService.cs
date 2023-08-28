using AutoMapper;
using Ayura.API.Features.EmailVerification.DTOs;
using Ayura.API.Features.EmailVerification.Helpers;
using Ayura.API.Features.EmailVerification.Services;
using Ayura.API.Features.Profile.Helpers.MailService;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IMongoCollection<EVC> _evcCollection;
    private readonly IMongoCollection<User> _userCollection;
    private readonly IMapper _mapper;
    private readonly IMailService _mailService;

    public EmailVerificationService(IAppSettings appSettings, IAyuraDatabaseSettings settings,
        IMongoClient mongoClient, IOptions<AppSettings> appSettingsOptions, IMailService mailService)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _evcCollection = database.GetCollection<EVC>(settings.EvcCollection);
        _userCollection = database.GetCollection<User>(settings.UserCollection);
        _appSettings = appSettingsOptions;
        _mailService = mailService;

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<EVCRequestDTO, EVC>().ReverseMap();
            cfg.CreateMap<EVCVerifyDTO, EVC>().ReverseMap();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    public async Task<bool> GenerateEmailVerificationCode(EVCRequestDTO evcRequestDto, string userId)
    {
        var evc = EVCGenerator.GenerateEVC();
        Console.Write("EVC Generated: " + evc + "\n");
        
        var evcModel = _mapper.Map<EVC>(evcRequestDto);
        
        Console.Write("EVC Model Created\n");
        
        // get user name
        var userFilter = Builders<User>.Filter.Eq("Id", userId);
        
        Console.Write("User Filter Created\n");
        User user = null;
        try
        {
            user = await _userCollection.Find(userFilter).FirstOrDefaultAsync();
            Console.Write("User Found\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error during Find user: " + ex.Message);
            // Handle the exception appropriately
        }
        var userName = user.FirstName + " " + user.LastName;
        Console.Write("User Name: " + userName + "\n");
        // validate DTO email with Database emails of user
        var emailFilter = Builders<User>.Filter.Eq("Email", evcRequestDto.email);
        User emailUser = await _userCollection.Find(emailFilter).FirstOrDefaultAsync();
        if (emailUser == null) return false;
        
        // validate both userid and email
        
        if (emailUser.Id != userId) return false;
        

        evcModel.Evc = evc;
        
        Console.Write("EVC Added to Model"+ evc + "\n");
        
        evcModel.UserId = userId;

        evcModel.ExpiryTime = DateTime.UtcNow.AddMinutes(10);

        Console.Write("EVC Expiry Time: " + evcModel.ExpiryTime + "\n");

        var filter = Builders<EVC>.Filter.Eq("UserId", userId);

        Console.Write("Filter Created\n");

        var evcFromDatabase = _evcCollection.Find(filter).FirstOrDefault();

        if (evcFromDatabase != null)
        {
            Console.Write("EVC from database is not null");
            _evcCollection.ReplaceOneAsync(filter, evcModel);
            Console.Write("EVC replaced\n");
        }
        else
        {
            Console.Write("EVC from database is null");
            _evcCollection.InsertOneAsync(evcModel);
            Console.Write("EVC inserted\n");
        }
        
        Console.Write("Database Updated\n");
        
        // Send email
        var mailData = new MailData()
        {
            EmailToId = evcRequestDto.email,
            EmailSubject = "Ayura Email Verification",
            EmailBody = $"Hello {userName},\n\nYour email verification code is {evc}.\n\nThis code will expire in 10 minutes.\n\nRegards,\nAyura Team"
        };
        Console.Write("Mail Data Created\n");
        Console.Write("Email To: " + mailData.EmailToId + "\n");
        Console.Write("Email Subject: " + mailData.EmailSubject + "\n");
        Console.Write("Email Body: " + mailData.EmailBody + "\n");
        
        return _mailService.SendMailAsync(mailData).Result;
    }

    public Task<string> VerifyEmail(EVCVerifyDTO evcVerifyDto, string userId)
    {
       
        var filter = Builders<EVC>.Filter.Eq("UserId", userId);
        var evcFromDatabase = _evcCollection.Find(filter).FirstOrDefault();

        if (evcFromDatabase == null) return Task.FromResult("No EVC found for this user");

        if (evcFromDatabase.Evc != evcVerifyDto.verificationCode) return Task.FromResult("EVC does not match");

        if (evcFromDatabase.ExpiryTime < DateTime.UtcNow) return Task.FromResult("EVC has expired");

        return Task.FromResult("EVC verified");
    }
}