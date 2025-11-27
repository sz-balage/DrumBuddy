using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using DrumBuddy.IO.Data;

namespace DrumBuddy.Api;

public class UserService : IUserService
{
    private static readonly byte[] EncryptionKey = new byte[]
        { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

    private readonly string _rememberMeFilePath;
    private readonly SheetRepository _repository;

    private string? _cachedToken;

    public UserService(SheetRepository repository)
    {
        _repository = repository;
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var drumBuddyFolder = Path.Combine(appDataPath, "DrumBuddy");

        if (!Directory.Exists(drumBuddyFolder))
            Directory.CreateDirectory(drumBuddyFolder);

        _rememberMeFilePath = Path.Combine(drumBuddyFolder, ".drumbuddy");
    }

    public string RefreshToken { get; private set; }

    public string? Email { get; private set; }
    public string? UserName { get; private set; }
    public bool IsOnline => !string.IsNullOrEmpty(Email) && IsTokenValid();
    public string? UserId { get; set; }

    public string? GetToken() => _cachedToken;

    public async Task SetToken(string token, string refreshToken, string userName, string email, string userId)
    {
        _cachedToken = token;
        RefreshToken = refreshToken;
        UserName = userName;
        Email = email;
        UserId = userId;
        await _repository.CreateUserIfNotExistsAsync(userId);
    }

    public void ClearToken()
    {
        _cachedToken = null;
        UserName = null;
        Email = null;
        UserId = null;
    }

    public bool IsTokenValid() => !string.IsNullOrEmpty(_cachedToken);

    public async Task SaveRememberedCredentialsAsync(string email, string password)
    {
        try
        {
            var credentials = new RememberedCredentials
            {
                Email = email,
                Password = password
            };
            var json = JsonSerializer.Serialize(credentials);
            var encryptedData = EncryptString(json);
            await File.WriteAllBytesAsync(_rememberMeFilePath, encryptedData);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to save remembered credentials: {ex.Message}");
        }
    }

    public async Task<(string? Email, string? Password)?> LoadRememberedCredentialsAsync()
    {
        try
        {
            if (!File.Exists(_rememberMeFilePath))
                return null;

            var encryptedData = await File.ReadAllBytesAsync(_rememberMeFilePath);
            var json = DecryptString(encryptedData);
            var credentials = JsonSerializer.Deserialize<RememberedCredentials>(json);

            return credentials != null ? (credentials.Email, credentials.Password) : null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load remembered credentials: {ex.Message}");
            return null;
        }
    }

    public void ClearRememberedCredentials()
    {
        try
        {
            if (File.Exists(_rememberMeFilePath))
                File.Delete(_rememberMeFilePath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to clear remembered credentials: {ex.Message}");
        }
    }

    private byte[] EncryptString(string plainText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);

                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(plainText);
                }

                return ms.ToArray();
            }
        }
    }

    private string DecryptString(byte[] cipherText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = EncryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            // Read IV from the beginning
            byte[] iv = new byte[aes.IV.Length];
            Array.Copy(cipherText, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream(cipherText, iv.Length, cipherText.Length - iv.Length))
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
            using (var sr = new StreamReader(cs))
            {
                return sr.ReadToEnd();
            }
        }
    }

    private class RememberedCredentials
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}