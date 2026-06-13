using Application.Dtos.SendEmail.AdminSide;
using Application.Services.Interfaces;
using Domain.Shared;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
namespace Application.Services.Implementaions
{
    public class SendEmailService : ISendEmailService
    {
        #region Fields
        private readonly IEmailDataService _emailDataService;
        #endregion

        #region Ctor
        public SendEmailService(IEmailDataService emailDataService)
        {
            _emailDataService = emailDataService;
        }
        #endregion

        public async Task<Result> AdminSideSendBuckEmail(AdminSideSendBuckEmailDto dto)
        {
            var emailSettingResult = await _emailDataService.AdminSideGetEmailData();
            if (emailSettingResult.IsFailure)
                return Result.Failure(emailSettingResult.Message);

            var emailSetting = emailSettingResult.Value;
            if (emailSetting == null)
                return Result.Failure(Messages.OperationFailedError);

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(emailSetting.EmailDisplayName, emailSetting.From));

                // 1. PRIVACY FIX: Put the sender in the "To" field, and recipients in "BCC". 
                // If you put everyone in "To", they will all see each other's email addresses.
                message.To.Add(new MailboxAddress(emailSetting.EmailDisplayName, emailSetting.From));

                foreach (var recipient in dto.Recipients ?? new List<string>())
                {
                    message.Bcc.Add(MailboxAddress.Parse(recipient));
                }

                message.Subject = dto.Subject;

                // 2. YOUR HTML WRAPPER (No ViewRenderService needed)
                string emailWrapper = @"
            <body style=""margin:0; padding:20px; font-family:Arial, sans-serif; background-color:#F9F9F9; color:#333;"">
                <div style=""max-width:600px; margin:auto; background-color:#fff; padding:30px; border-radius:10px; box-shadow:0 0 10px rgba(0,0,0,0.05);"">
                    {0}
                </div>
            </body>";

                // 3. INJECT THE DTO BODY
                string finalHtml = string.Format(emailWrapper, dto.Body);

                // 4. SPAM FILTER PROTECTION: Use BodyBuilder to include plain text
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = finalHtml,
                    TextBody = "To view this newsletter, please open it in an HTML-compatible email client."
                };
                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();

                // Use 'Auto' instead of 'true'. 'true' forces strict SSL which often fails on port 587.
                await client.ConnectAsync(emailSetting.Smtp, emailSetting.Port, SecureSocketOptions.Auto);
                await client.AuthenticateAsync(emailSetting.From, emailSetting.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                return Result.Success(Messages.SuccessfullyDone);
            }
            catch (Exception e)
            {
                return Result.Failure(e.Message);
            }
        }
    }
}
