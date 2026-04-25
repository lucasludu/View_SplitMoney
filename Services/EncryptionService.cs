using System.Security.Cryptography;
using System.Text;

namespace SplitMoney.Client.Services
{
    public class EncryptionService : IEncryptionService
    {
        private const string KEY_STORAGE_NAME = "app_master_encryption_key";
        private byte[]? _key;

        private async Task EnsureKeyAsync()
        {
            if (_key != null) return;

            var savedKey = await SecureStorage.Default.GetAsync(KEY_STORAGE_NAME);
            if (string.IsNullOrEmpty(savedKey))
            {
                // Generar una llave aleatoria de 32 bytes (256 bits)
                var newKey = new byte[32];
                RandomNumberGenerator.Fill(newKey);
                savedKey = Convert.ToBase64String(newKey);
                await SecureStorage.Default.SetAsync(KEY_STORAGE_NAME, savedKey);
            }

            _key = Convert.FromBase64String(savedKey);
        }

        public async Task<string> EncryptAsync(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            await EnsureKeyAsync();

            using Aes aes = Aes.Create();
            aes.Key = _key!;
            aes.GenerateIV();
            byte[] iv = aes.IV;

            using var encryptor = aes.CreateEncryptor(aes.Key, iv);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

            // Resultado: IV (16 bytes) + Datos Encriptados
            byte[] result = new byte[iv.Length + encryptedBytes.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encryptedBytes, 0, result, iv.Length, encryptedBytes.Length);

            return Convert.ToBase64String(result);
        }

        public async Task<string> DecryptAsync(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try 
            {
                await EnsureKeyAsync();

                byte[] fullCipher = Convert.FromBase64String(cipherText);
                
                using Aes aes = Aes.Create();
                aes.Key = _key!;
                
                byte[] iv = new byte[16];
                byte[] cipherBytes = new byte[fullCipher.Length - 16];
                
                Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);
                
                aes.IV = iv;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (Exception)
            {
                // Si falla la desencriptación (ej: llave corrupta), devolvemos vacío o manejamos el error
                return string.Empty;
            }
        }
    }
}
