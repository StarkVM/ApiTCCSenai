namespace UserAccess.Domain.Interfaces;

public interface ICpfHasher
{
    string Hash(string cpfClean11Digits);
}