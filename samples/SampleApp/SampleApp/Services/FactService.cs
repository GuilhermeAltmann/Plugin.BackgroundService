using System.Threading.Tasks;
using Plugin.BackgroundService;
using Xamarin.Forms;

namespace SampleApp.Services
{
    public interface IFactService
    {
        Task UpdateFactAsync();
        Task<FactModel> GetLatestFactAsync();
    }

    public class FactService : IFactService
    {
        private readonly IRestService _restService;
        private readonly ISecureStorageService _secureStorageService;
        private readonly IBackgroundService _backgroundService;

        public FactService(IRestService restService, ISecureStorageService secureStorageService, IBackgroundService backgroundService)
        {
            _restService = restService;
            _secureStorageService = secureStorageService;
            _backgroundService = backgroundService;
        }

        public async Task UpdateFactAsync()
        {
            var fact = await _restService.GetFactAsync();
            await _secureStorageService.SetAsync("fact", fact);
            _backgroundService.UpdateNotificationMessage(fact.Text);
        }

        public async Task<FactModel> GetLatestFactAsync()
        {
            var fact = await _secureStorageService.GetAsync<FactModel>("fact");
            return fact;
        }
    }
}
