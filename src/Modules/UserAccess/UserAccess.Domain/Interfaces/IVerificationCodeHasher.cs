namespace UserAccess.Domain.Interfaces;

public interface IVerificationCodeHasher
{
    string Hash(string code);
}