using AutoMapper;
using Ayura.API.Configuration;
using Ayura.API.Features.OTP.DTOs;
using Ayura.API.Features.OTP.Helpers;
using Ayura.API.Features.OTP.Models;
using Ayura.API.Features.OTP.Services;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ayura.API.Services;

public class OtpService : IOtpService
{
    private readonly IOptions<AppSettings> _appSettings;
    private readonly IMapper _mapper;
    private readonly IMongoCollection<Otp> _otpCollection;

    public OtpService(IAppSettings appSettings, IAyuraDatabaseSettings settings, IMongoClient mongoClient,
        IOptions<AppSettings> appSettingsOptions)
    {
        // database and collections setup
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _otpCollection = database.GetCollection<Otp>(settings.OtpCollection);
        _appSettings = appSettingsOptions;

        // DTO to model mapping setup
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OtpRequestDto, Otp>().ReverseMap();
            cfg.CreateMap<OtpVerifierDto, Otp>().ReverseMap();
        });

        _mapper = mapperConfig.CreateMapper();
    }

    public Task<string> GenerateOtp(OtpRequestDto otpRequestDTO)
    {
        var otp = OtpGenerator.GenerateOtp();

        // mapping the otpRequestDTO to OTP model 
        var otpModel = _mapper.Map<Otp>(otpRequestDTO);

        // save the otp to the model
        otpModel.OtpNum = otp;

        // set the expiry time
        otpModel.ExpiryTime = DateTime.UtcNow.AddMinutes(10);

        Console.Write("OTP Expiry Time: " + otpModel.ExpiryTime + "\n");

        // if mobile number exists in otp database, replace otp
        var filter = Builders<Otp>.Filter.Eq("MobileNumber", otpModel.MobileNumber);

        // if the mobile number exists, replace the otp
        var otpFromDatabase = _otpCollection.Find(filter).FirstOrDefault();

        if (otpFromDatabase != null)
            // replace the otp
            _otpCollection.ReplaceOne(filter, otpModel);

        else
            // save the otp to the database
            _otpCollection.InsertOne(otpModel);

        // send to the mobile

        // Return successful message
        return Task.FromResult("Otp sent successfully");
    }

    public Task<string> VerifyOtp(OtpVerifierDto otpVerifierDto)
    {
        // mapping the otpVerifierDTO to OTP model
        var otpModel = _mapper.Map<Otp>(otpVerifierDto);

        // check if an otp exists for the given mobileNumber
        var filter = Builders<Otp>.Filter.Eq("MobileNumber", otpModel.MobileNumber);

        // get the otp from the database
        var otp = _otpCollection.Find(filter).FirstOrDefault();
        Console.Write("OTP Expiry Time: " + otp.ExpiryTime + "\n");
        // compare the otp
        if (otp.OtpNum == null)
            // otp does not exist
            return Task.FromResult("Otp does not exist");
        if (otp.OtpNum != otpModel.OtpNum)
            // otp does not match
            return Task.FromResult("Otp does not match");
        if (otp.ExpiryTime < DateTime.UtcNow)
            // otp expired
            return Task.FromResult("Otp expired");
        // otp verified
        return Task.FromResult("Otp verified");
    }
}