using System.Threading.Tasks;
using Plugin.BackgroundService.Messages;
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
        private readonly IMessagingCenter _messagingCenter;

        public FactService(IRestService restService, ISecureStorageService secureStorageService, IMessagingCenter messagingCenter)
        {
            _restService = restService;
            _secureStorageService = secureStorageService;
            _messagingCenter = messagingCenter;
        }

        public async Task UpdateFactAsync()
        {
            var fact = await _restService.GetFactAsync();
            await _secureStorageService.SetAsync("fact", fact);
            _messagingCenter.Send<object, UpdateNotificationMessage>(this, ToBackgroundMessages.UpdateBackgroundServiceNotificationMessage, new UpdateNotificationMessage(fact.Text));
        }

        public async Task<FactModel> GetLatestFactAsync()
        {
            var fact = await _secureStorageService.GetAsync<FactModel>("fact");
            return fact;
        }
    }
}
