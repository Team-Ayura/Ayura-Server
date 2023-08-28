using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Features.Profile.Helpers.MailService;

[ApiController]
[Route("api/mail")]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;
    //injecting the IMailService into the constructor
    public MailController(IMailService _MailService)
    {
        _mailService = _MailService;
    }

    [HttpPost]
    [Route("SendMail")]
    public bool SendMail(MailData mailData)
    {
        return _mailService.SendMailAsync(mailData).Result;
    }
}