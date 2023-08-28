using Ayura.API.Features.EmailVerification.DTOs;

namespace Ayura.API.Features.EmailVerification.Services;

public interface IEmailVerificationService
{
    // Generate email verification code and store in the database with user id
    Task<bool> GenerateEmailVerificationCode(EVCRequestDTO evcRequestDto, string userId);

    // Verify email verification code and return true if it matches
    Task<string> VerifyEmail(EVCVerifyDTO evcVerifyDto, string userId);
}