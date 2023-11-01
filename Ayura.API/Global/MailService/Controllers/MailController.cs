using Ayura.API.Features.Profile.Helpers.MailService;
using Ayura.API.Global.MailService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Ayura.API.Global.MailService.Controllers;

[ApiController]
[Route("api/mail")]
public class MailController : ControllerBase
{
    private readonly IMailService _mailService;

    // create a test maildata for Name Pasan Gimhana and Email Pasangimhana@gmail.com
    // and send it to the SendMail method

    private readonly MailData mailData = new()
    {
        EmailToId = "pasangimhanaofficial@gmail.com", EmailToName = "Pasan Gimhana", EmailSubject = "Test Mail",
        EmailBody = "This is a test mail"
    };

    //injecting the IMailService into the constructor
    public MailController(IMailService mailService)
    {
        _mailService = mailService;
    }

    [HttpPost]
    [Route("send")]
    public bool SendMail()
    {
        return _mailService.SendMailAsync(mailData).Result;
    }
}