using Ayura.API.Features.OTP.DTOs;

namespace Ayura.API.Features.OTP.Services;

public interface IOtpService
{
    Task<string> GenerateOtp(OtpRequestDTO otpRequestDTO);
    
    Task<string> VerifyOtp(OtpVerifierDTO otpVerifierDTO);
    
}