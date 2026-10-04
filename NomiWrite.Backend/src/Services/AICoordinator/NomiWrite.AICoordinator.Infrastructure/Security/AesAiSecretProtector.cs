using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using NomiWrite.AICoordinator.Application.Interfaces;
using NomiWrite.AICoordinator.Infrastructure.Options;

namespace NomiWrite.AICoordinator.Infrastructure.Security;

public sealed class AesAiSecretProtector : IAiSecretProtector
{
    private const string Version = "v1";
    private readonly byte[]? _masterKey;

    public AesAiSecretProtector(IOptions<AiSecretProtectionSettings> settings)
    {
        var configuredKey = settings.Value.MasterKey?.Trim();
        if (string.IsNullOrEmpty(configuredKey))
            return;

        try
        {
            var decoded = Convert.FromBase64String(configuredKey);
            if (decoded.Length != 32)
                throw new InvalidOperationException("AiSecretProtection:MasterKey must decode to exactly 32 bytes.");

            _masterKey = decoded;
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("AiSecretProtection:MasterKey must be a valid base64 value.", exception);
        }
    }

    public bool IsConfigured => _masterKey is not null;

    public string Protect(string plaintext)
    {
        EnsureConfigured();
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);

        var nonce = RandomNumberGenerator.GetBytes(AesGcm.NonceByteSizes.MaxSize);
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        using var aes = new AesGcm(_masterKey!, tag.Length);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        return string.Join('.', Version, Convert.ToBase64String(nonce), Convert.ToBase64String(ciphertext), Convert.ToBase64String(tag));
    }

    public string Unprotect(string protectedValue)
    {
        EnsureConfigured();
        var parts = protectedValue.Split('.');
        if (parts.Length != 4 || parts[0] != Version)
            throw new CryptographicException("The stored AI secret has an unsupported format.");

        try
        {
            var nonce = Convert.FromBase64String(parts[1]);
            var ciphertext = Convert.FromBase64String(parts[2]);
            var tag = Convert.FromBase64String(parts[3]);
            var plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(_masterKey!, tag.Length);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }
        catch (FormatException exception)
        {
            throw new CryptographicException("The stored AI secret is malformed.", exception);
        }
    }

    private void EnsureConfigured()
    {
        if (!IsConfigured)
            throw new InvalidOperationException("AI secret protection is not configured. Set AiSecretProtection__MasterKey in the service environment.");
    }
}
