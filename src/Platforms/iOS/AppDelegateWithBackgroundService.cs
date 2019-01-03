using System;
using Plugin.BackgroundService.Messages;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

namespace Plugin.BackgroundService
{
    /// <summary>
    /// AppDelegate to inherit from if you want to handle background service host
    /// </summary>
    public abstract class AppDelegateWithBackgroundService : FormsApplicationDelegate
    {
        private NativeBackgroundServiceHost BackgroundService { get; set; }

        private IMessagingCenter _messagingCenter;

        /// <inheritdoc />
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            var result = base.FinishedLaunching(uiApplication, launchOptions);

            _messagingCenter = MessagingCenter.Instance;
            _messagingCenter.Subscribe<object>(this, ToBackgroundMessages.StartBackgroundService, OnStartBackgroundServiceMessage);
            _messagingCenter.Subscribe<object>(this, ToBackgroundMessages.StopBackgroundService, OnStopBackgroundServiceMessage);
            _messagingCenter.Subscribe<object>(this, ToBackgroundMessages.GetBackgroundServiceState, OnGetBackgroundServiceState);

            BackgroundService = new NativeBackgroundServiceHost();

            return result;
        }

        private void OnStartBackgroundServiceMessage(object obj)
        {
            if (BackgroundService != null && BackgroundService.IsStarted)
                return;
            if (BackgroundService == null)
                throw new InvalidOperationException("BackgroundService is not instantiated");
            BackgroundService.Start();
        }

        private void OnStopBackgroundServiceMessage(object obj)
        {
            if (BackgroundService != null && !BackgroundService.IsStarted)
                return;
            if (BackgroundService == null)
                throw new InvalidOperationException("BackgroundService is not instantiated");
            BackgroundService.Stop();
        }

        private void OnGetBackgroundServiceState(object obj)
        {
            var msg = new BackgroundServiceState(BackgroundService != null && BackgroundService.IsStarted);
            _messagingCenter.Send<object, BackgroundServiceState>(this, FromBackgroundMessages.BackgroundServiceState, msg);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            _messagingCenter.Unsubscribe<object>(this, ToBackgroundMessages.StartBackgroundService);
            _messagingCenter.Unsubscribe<object>(this, ToBackgroundMessages.StopBackgroundService);
            _messagingCenter.Unsubscribe<object>(this, ToBackgroundMessages.GetBackgroundServiceState);

            base.Dispose(disposing);
        }
    }
}