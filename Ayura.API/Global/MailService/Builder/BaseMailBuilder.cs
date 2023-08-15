namespace Ayura.API.Features.Profile.Helpers.MailService;

public class BaseMailBuilder : IMailBuilder
{
private MailData _mailData = new MailData();

    public IMailBuilder SetEmailBody(string emailBody)
    {
        _mailData.EmailBody = emailBody;
        return this;
    }

    public IMailBuilder SetEmailSubject(string emailSubject)
    {
        _mailData.EmailSubject = emailSubject;
        return this;
    }

    public IMailBuilder SetEmailToName(string emailToName)
    {
        _mailData.EmailToName = emailToName;
        return this;
    }

    public IMailBuilder SetEmailToId(string emailToId)
    {
        _mailData.EmailToId = emailToId;
        return this;
    }

    public MailData Build()
    {
        return _mailData;
    }
    
}