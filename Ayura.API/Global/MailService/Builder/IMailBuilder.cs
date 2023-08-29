using Ayura.API.Global.MailService.DTOs_;

namespace Ayura.API.Features.Profile.Helpers.MailService;

public interface IMailBuilder
{
    public IMailBuilder SetEmailBody(string emailBody);
    public IMailBuilder SetEmailSubject(string emailSubject);
    public IMailBuilder SetEmailToName(string emailToName);
    public IMailBuilder SetEmailToId(string emailToId);
    
    public MailData Build();
}