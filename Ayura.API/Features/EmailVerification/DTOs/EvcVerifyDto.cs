namespace Ayura.API.Features.EmailVerification.DTOs;

public class EvcVerifyDto
{
    public string Email { get; set; } = null!;
    public string VerificationCode { get; set; } = null!;
}