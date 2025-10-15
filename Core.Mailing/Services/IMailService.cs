using Mail = ZCode.Core.Mailing.Models.Mail;

namespace ZCode.Core.Mailing.Services;

public interface IMailService
{
    void SendMail(Mail mail);
    Task SendEmailAsync(Mail mail);
}

