using UserAccess.Domain.Enums;
using UserAccess.Domain.Senders;

namespace UserAccess.Domain.Interfaces;

public interface IVerificationCodeSender
{
    Task SendCodeAsync (SendVerificationCodeRequest verificationCodeCommand, CancellationToken cancellationToken);
}