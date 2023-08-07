using System.Runtime.InteropServices.ComTypes;
using AutoMapper;
using Ayura.API.Features.OTP.DTOs;
using Ayura.API.Features.OTP.Services;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Ayura.API.Services
{
    public class OtpService : IOtpService
    {
        private readonly IMongoCollection<OTP> _otpCollection;
        private readonly IOptions<AppSettings> _appSettings;
        private readonly IMapper _mapper;
        
        public OtpService(IAppSettings appSettings, IAyuraDatabaseSettings settings, IMongoClient mongoClient, IOptions<AppSettings> appSettingsOptions)
        {
            // database and collections setup
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _otpCollection = database.GetCollection<OTP>(settings.OtpCollection);
            _appSettings = appSettingsOptions;
            
            // DTO to model mapping setup
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OtpRequestDTO, OTP>().ReverseMap();
                cfg.CreateMap<OtpVerifierDTO, OTP>().ReverseMap();
                
            });

            _mapper = mapperConfig.CreateMapper();
        }
        
        public Task<string> GenerateOtp(OtpRequestDTO otpRequestDTO)
        {
            throw new NotImplementedException();
        }

        public Task<string> VerifyOtp(OtpVerifierDTO otpVerifierDTO)
        {
            throw new NotImplementedException();
        }
    }
    
}