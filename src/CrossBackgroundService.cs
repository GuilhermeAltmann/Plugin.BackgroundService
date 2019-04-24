using System;
using System.Threading;
using Plugin.BackgroundService.Messages;
using Xamarin.Forms;

namespace Plugin.BackgroundService
{
    public class CrossBackgroundService : IBackgroundService
    {
        private static readonly Lazy<IBackgroundService> Instance = new Lazy<IBackgroundService>(() => new CrossBackgroundService(), LazyThreadSafetyMode.PublicationOnly);

        public static IBackgroundService Current
        {
            get
            {
                var value = Instance.Value;
                if (value == null)
                    throw new NotImplementedException();
                return value;
            }
        }

        public bool IsRunning
        {
            get => _isRunning;
            private set
            {
                if (value == _isRunning)
                    return;
                _isRunning = value;
                OnBackgroundServiceRunningStateChanged();
            }
        }

        public event EventHandler<BackgroundServiceRunningStateChangedEventArgs> BackgroundServiceRunningStateChanged;

        private readonly IMessagingCenter _messagingCenter;
        private bool _isRunning;

        private CrossBackgroundService()
        {
            _messagingCenter = MessagingCenter.Instance;
            _messagingCenter.Subscribe<object, BackgroundServiceState>(this,
                FromBackgroundMessages.BackgroundServiceState, OnBackgroundServiceStateMessage);
            _messagingCenter.Send<object>(this, ToBackgroundMessages.GetBackgroundServiceState);
        }

        private void OnBackgroundServiceStateMessage(object sender, BackgroundServiceState state)
        {
            IsRunning = state.IsRunning;
        }

        public void StartService()
        {
            _messagingCenter.Send<object>(this, ToBackgroundMessages.StartBackgroundService);
        }

        public void StopService()
        {
            _messagingCenter.Send<object>(this, ToBackgroundMessages.StopBackgroundService);
        }

        public void UpdateNotificationMessage(string newText)
        {
            _messagingCenter.Send<object, UpdateNotificationMessage>(this, ToBackgroundMessages.UpdateBackgroundServiceNotificationMessage, new UpdateNotificationMessage(newText));
        }

        public void Dispose()
        {
            _messagingCenter.Unsubscribe<object, BackgroundServiceState>(this,
                FromBackgroundMessages.BackgroundServiceState);
        }

        private void OnBackgroundServiceRunningStateChanged()
        {
            BackgroundServiceRunningStateChanged?.Invoke(this, new BackgroundServiceRunningStateChangedEventArgs(IsRunning));
        }
    }
}
