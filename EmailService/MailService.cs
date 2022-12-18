using CleanScheduler.API.Configuration;
using EmailService.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailService
{
    public class MailService : IMailService
    {
        private readonly MailOptions _mailOptions;

        public MailService(IOptions<MailOptions> mailOptions)
        {
            _mailOptions = mailOptions.Value;
        }

        public async Task<bool> SendAsync(MailModel mailModel, CancellationToken cancellationToken)
        {
            try
            {
                // Initialize a new instance of the MimeKit.MimeMessage class
                var mail = new MimeMessage();

                #region Sender / Receiver
                // Sender
                mail.From.Add(new MailboxAddress(_mailOptions.DisplayName, mailModel.From ?? _mailOptions.From));
                mail.Sender = new MailboxAddress(mailModel.DisplayName ?? _mailOptions.DisplayName, mailModel.From ?? _mailOptions.From);

                // Receiver
                foreach (string mailAddress in mailModel.To)
                    mail.To.Add(MailboxAddress.Parse(mailAddress));

                // Set Reply to if specified in mail data
                if (!string.IsNullOrEmpty(mailModel.ReplyTo))
                    mail.ReplyTo.Add(new MailboxAddress(mailModel.ReplyToName, mailModel.ReplyTo));

                // BCC
                // Check if a BCC was supplied in the request
                if (mailModel.Bcc != null)
                {
                    // Get only addresses where value is not null or with whitespace. x = value of address
                    foreach (string mailAddress in mailModel.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                        mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                }

                // CC
                // Check if a CC address was supplied in the request
                if (mailModel.Cc != null)
                {
                    foreach (string mailAddress in mailModel.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                        mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                }
                #endregion

                #region Content

                // Add Content to Mime Message
                var body = new BodyBuilder();
                mail.Subject = mailModel.Subject;
                body.HtmlBody = mailModel.Body;
                mail.Body = body.ToMessageBody();

                #endregion

                #region Send Mail

                using var smtp = new SmtpClient();

                if (_mailOptions.UseSSL)
                {
                    await smtp.ConnectAsync(_mailOptions.Host, _mailOptions.Port, SecureSocketOptions.SslOnConnect, cancellationToken);
                }
                else if (_mailOptions.UseStartTls)
                {
                    await smtp.ConnectAsync(_mailOptions.Host, _mailOptions.Port, SecureSocketOptions.StartTls, cancellationToken);
                }
                await smtp.AuthenticateAsync(_mailOptions.UserName, _mailOptions.Password, cancellationToken);
                await smtp.SendAsync(mail, cancellationToken);
                await smtp.DisconnectAsync(true, cancellationToken);

                #endregion

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
