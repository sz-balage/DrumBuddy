using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DrumBuddy.Services;

public class TokenService
{
    // In-memory storage (lost on app close)
    private string? _cachedToken;
    private string? _cachedUserId;
    private string? _cachedUserEmail;

    private readonly string _rememberMeFilePath;
    private static readonly byte[] _encryptionKey = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 };

    public TokenService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var drumBuddyFolder = Path.Combine(appDataPath, "DrumBuddy");
        
        if (!Directory.Exists(drumBuddyFolder))
            Directory.CreateDirectory(drumBuddyFolder);

        _rememberMeFilePath = Path.Combine(drumBuddyFolder, ".drumbuddy");
    }

    // Session token (in-memory only)
    public async Task<string?> GetTokenAsync() => _cachedToken;

    public async Task SetTokenAsync(string token, string userId, string email)
    {
        _cachedToken = token;
        _cachedUserId = userId;
        _cachedUserEmail = email;
    }

    public async Task ClearTokenAsync()
    {
        _cachedToken = null;
        _cachedUserId = null;
        _cachedUserEmail = null;
    }

    public string? GetCachedUserId() => _cachedUserId;
    public string? GetCachedUserEmail() => _cachedUserEmail;
    public bool IsTokenValid() => !string.IsNullOrEmpty(_cachedToken);

    // "Remember Me" encrypted storage
    public async Task SaveRememberedCredentialsAsync(string email, string password)
    {
        try
        {
            var credentials = new { email, password };
            var json = JsonSerializer.Serialize(credentials);
            var encryptedData = EncryptString(json);
            await File.WriteAllBytesAsync(_rememberMeFilePath, encryptedData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save remembered credentials: {ex.Message}");
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
            System.Diagnostics.Debug.WriteLine($"Failed to load remembered credentials: {ex.Message}");
            return null;
        }
    }

    public async Task ClearRememberedCredentialsAsync()
    {
        try
        {
            if (File.Exists(_rememberMeFilePath))
                File.Delete(_rememberMeFilePath);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear remembered credentials: {ex.Message}");
        }
    }

    private byte[] EncryptString(string plainText)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = _encryptionKey;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
            using (var ms = new MemoryStream())
            {
                // Write IV to the beginning of the stream
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
            aes.Key = _encryptionKey;
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
