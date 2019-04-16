using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace SampleApp.Services
{
    public interface ISecureStorageService
    {
        Task SetAsync<T>(string key, T value);
        Task<T> GetAsync<T>(string key);
    }

    public class SecureStorageService : ISecureStorageService
    {
        public async Task SetAsync<T>(string key, T value)
        {
            var serializedObject = JsonConvert.SerializeObject(value);
            await SecureStorage.SetAsync(key, serializedObject);
        }

        public async Task<T> GetAsync<T>(string key)
        {
            var serializedObject = await SecureStorage.GetAsync(key);
            if (serializedObject == null)
                return default;
            return JsonConvert.DeserializeObject<T>(serializedObject);
        }
    }
}
