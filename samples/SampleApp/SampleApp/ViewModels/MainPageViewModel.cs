using System;
using System.Windows.Input;
using Plugin.BackgroundService;
using Prism.AppModel;
using Prism.Commands;
using Prism.Navigation;

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

        private readonly IBackgroundService _backgroundService;

        public MainPageViewModel(INavigationService navigationService, IBackgroundService backgroundService)
            : base(navigationService)
        {
            Title = "Main Page";
            _backgroundService = backgroundService;
            BackgroundRunning = false;
            StartServiceCommand = new DelegateCommand(StartService, () => !BackgroundRunning).ObservesProperty(() => BackgroundRunning);
            StopServiceCommand = new DelegateCommand(StopService, () => BackgroundRunning).ObservesProperty(() => BackgroundRunning);
        }

        private void StartService()
        {
            _backgroundService.StartService();
        }

        private void StopService()
        {
            _backgroundService.StopService();
        }

        public void OnAppearing()
        {
            _backgroundService.BackgroundServiceRunningStateChanged += OnBackgroundServiceStateChanged;
        }

        private void OnBackgroundServiceStateChanged(object sender, BackgroundServiceRunningStateChangedEventArgs eventArgs)
        {
            BackgroundRunning = eventArgs.IsRunning;
        }

        public void OnDisappearing()
        {
            _backgroundService.BackgroundServiceRunningStateChanged -= OnBackgroundServiceStateChanged;
        }
    }
}