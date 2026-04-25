using System.Threading.Tasks;

namespace SplitMoney.Client.Services
{
    public interface IEncryptionService
    {
        Task<string> EncryptAsync(string plainText);
        Task<string> DecryptAsync(string cipherText);
    }
}
