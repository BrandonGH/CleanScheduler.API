using EmailService.Models;

namespace EmailService
{
    public interface IMailService
    {
        Task<bool> SendAsync(MailModel mailModel, CancellationToken cancellationToken);
    }
}
