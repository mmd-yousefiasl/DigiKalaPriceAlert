using Domain.Enums.ContactUs;
using MediatR;

namespace Domain.Events.ContactUs
{
    public record ContactUsFormCreatedEvent(
    DateTime CreatedDate,
    string FullName,
    string Mobile,
    ContactUsFormSubjectEnum Subject ,
    string Message ,
    bool IsRead ,
    int? ReadByUserId ,
    int? UserId ,
    string UserIP
    ) : INotification;
}
