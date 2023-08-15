namespace Ayura.API.Features.Profile.Helpers.MailService;

public interface IMailService
{
    Task<bool> SendMailAsync(MailData mailData);
}