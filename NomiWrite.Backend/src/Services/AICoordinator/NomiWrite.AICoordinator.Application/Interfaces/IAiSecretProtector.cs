namespace NomiWrite.AICoordinator.Application.Interfaces;

public interface IAiSecretProtector
{
    bool IsConfigured { get; }
    string Protect(string plaintext);
    string Unprotect(string ciphertext);
}
