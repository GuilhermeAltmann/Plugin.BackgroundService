using System.Windows.Input;
using Plugin.BackgroundService.Messages;
using Prism.AppModel;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Forms;

namespace SampleApp.ViewModels
{
    public class MainPageViewModel : ViewModelBase, IPageLifecycleAware
    {
        public ICommand StartServiceCommand { get; }
        public ICommand StopServiceCommand { get; }

        public bool BackgroundRunning
        {
            get => _backgroundRunning;
            set => SetProperty(ref _backgroundRunning, value);
        }

        private bool _backgroundRunning;

        private readonly IMessagingCenter _messagingCenter;

        public MainPageViewModel(INavigationService navigationService, IMessagingCenter messagingCenter)
            : base(navigationService)
        {
            Title = "Main Page";
            _messagingCenter = messagingCenter;
            BackgroundRunning = false;
            StartServiceCommand = new DelegateCommand(StartService, () => !BackgroundRunning).ObservesProperty(() => BackgroundRunning);
            StopServiceCommand = new DelegateCommand(StopService, () => BackgroundRunning).ObservesProperty(() => BackgroundRunning);
        }

        private void StartService()
        {
            _messagingCenter.Send<object>(this, ToBackgroundMessages.StartBackgroundService);
        }

        private void StopService()
        {
            _messagingCenter.Send<object>(this, ToBackgroundMessages.StopBackgroundService);
        }

        public void OnAppearing()
        {
            _messagingCenter.Subscribe<object, BackgroundServiceState>(this, FromBackgroundMessages.BackgroundServiceState, OnBackgroundServiceState);
            _messagingCenter.Send<object>(this, ToBackgroundMessages.GetBackgroundServiceState);
        }

        private void OnBackgroundServiceState(object sender, BackgroundServiceState state)
        {
            BackgroundRunning = state.IsRunning;
        }

        public void OnDisappearing()
        {
            _messagingCenter.Unsubscribe<object, BackgroundServiceState>(this, FromBackgroundMessages.BackgroundServiceState);
        }
    }
}