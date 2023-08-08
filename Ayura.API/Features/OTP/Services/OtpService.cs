using System.Runtime.InteropServices.ComTypes;
using AutoMapper;
using Ayura.API.Features.OTP.DTOs;
using Ayura.API.Features.OTP.Helpers;
using Ayura.API.Features.OTP.Services;
using Ayura.API.Models;
using Ayura.API.Models.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
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
            string otp = OtpGenerator.GenerateOtp();

            // mapping the otpRequestDTO to OTP model 
            var otpModel = _mapper.Map<OTP>(otpRequestDTO);

            // save the otp to the model
            otpModel.Otp = otp;

            // set the expiry time
            otpModel.ExpiryTime = DateTime.UtcNow.AddMinutes(10);
            
            Console.Write("OTP Expiry Time: " + otpModel.ExpiryTime + "\n");

            // if mobile number exists in otp database, replace otp
            var filter = Builders<OTP>.Filter.Eq("MobileNumber", otpModel.MobileNumber);

            // if the mobile number exists, replace the otp
            var otpFromDatabase = _otpCollection.Find(filter).FirstOrDefault();

            if (otpFromDatabase != null)
            {
                // replace the otp
                _otpCollection.ReplaceOne(filter, otpModel);
            }

            else
            {
                // save the otp to the database
                _otpCollection.InsertOne(otpModel);
            }
            
            // send to the mobile
            
            // Return successful message
            return Task.FromResult("Otp sent successfully");
        }

        public Task<string> VerifyOtp(OtpVerifierDTO otpVerifierDTO)
        {
            // mapping the otpVerifierDTO to OTP model
            var otpModel = _mapper.Map<OTP>(otpVerifierDTO);
            
            // check if an otp exists for the given mobileNumber
            var filter = Builders<OTP>.Filter.Eq("MobileNumber", otpModel.MobileNumber);
            
            // get the otp from the database
            var otp = _otpCollection.Find(filter).FirstOrDefault();
            Console.Write("OTP Expiry Time: " + otp.ExpiryTime + "\n");
            // compare the otp
            if (otp.Otp == null)
            {
                // otp does not exist
                return Task.FromResult("Otp does not exist");
            }
            else if (otp.Otp != otpModel.Otp)
            {
                // otp does not match
                return Task.FromResult("Otp does not match");
            }
            else if (otp.ExpiryTime < DateTime.UtcNow)
            {
                // otp expired
                return Task.FromResult("Otp expired");
            }
            else
            {
                // otp verified
                return Task.FromResult("Otp verified");
            }
        }
    }
    
}