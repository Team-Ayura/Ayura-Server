using Ayura.API.Global.MailService.Configuration;
using Ayura.API.Global.MailService.DTOs;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Ayura.API.Features.Profile.Helpers.MailService;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;

    public MailService(IOptions<MailSettings> mailSettingsOptions)
    {
        _mailSettings = mailSettingsOptions.Value;
    }

    public async Task<bool> SendMailAsync(MailData mailData)
    {
        try
        {
            using (var emailMessage = new MimeMessage())
            {
                var emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);
                emailMessage.From.Add(emailFrom);
                var emailTo = new MailboxAddress(mailData.EmailToName, mailData.EmailToId);
                emailMessage.To.Add(emailTo);

                // you can add the CCs and BCCs here.
                //emailMessage.Cc.Add(new MailboxAddress("Cc Receiver", "cc@example.com"));
                //emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", "bcc@example.com"));

                emailMessage.Subject = mailData.EmailSubject;

                var emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.TextBody = mailData.EmailBody;

                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                //this is the SmtpClient from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                using (var mailClient = new SmtpClient())
                {
                    await mailClient.ConnectAsync(_mailSettings.Server, _mailSettings.Port,
                        SecureSocketOptions.StartTls);
                    await mailClient.AuthenticateAsync(_mailSettings.UserName, _mailSettings.Password);
                    await mailClient.SendAsync(emailMessage);
                    await mailClient.DisconnectAsync(true);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            // Exception Details
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}