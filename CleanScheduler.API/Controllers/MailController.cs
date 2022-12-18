using EmailService;
using EmailService.Models;
using Microsoft.AspNetCore.Mvc;

namespace CleanScheduler.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;

        public MailController(IMailService mailService)
        {
            _mailService = mailService;
        }

        [HttpPost(Name = "PostMail")]
        public async Task<IActionResult> PostMail(MailModel mail)
        {
            bool result = await _mailService.SendAsync(mail, new CancellationToken());

            if (result)
            {
                return StatusCode(StatusCodes.Status200OK, "Mail has successfully been sent.");
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occured. The Mail could not be sent.");
            }
        }
    }
}
