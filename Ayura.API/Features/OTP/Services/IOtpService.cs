using Ayura.API.Features.OTP.DTOs;

namespace Ayura.API.Features.OTP.Services;

public interface IOtpService
{
    Task<string> GenerateOtp(OtpRequestDto otpRequestDto);

    Task<string> VerifyOtp(OtpVerifierDto otpVerifierDto);
}