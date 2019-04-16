using System.Collections.Generic;
using Android.Content;
using Android.OS;
using Android.Provider;
using Plugin.BackgroundService.Messages;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

// ReSharper disable once CheckNamespace
namespace Plugin.BackgroundService
{
    /// <summary>
    /// Inherit from this activity for handling background services
    /// </summary>
    public abstract class ActivityWithBackgroundService : FormsAppCompatActivity
    {
        /// <summary>
        /// Tag for logging purpose
        /// </summary>
        public virtual string Tag => Application.PackageName;


        /// <summary>
        /// Define if we should ask the user to turn down battery optimizations on startup
        /// </summary>
        /// <example>
        /// protected override bool AskForBatteryOptimizations
        /// {
        ///   get
        ///   {
        ///   #if DEBUG
        ///     return false;
        ///   #else
        ///     return true;
        ///   #endif
        ///   }
        /// }
        /// </example>
        protected virtual bool AskForBatteryOptimizations => true;

        internal NativeBackgroundServiceHost BackgroundService
        {
            get => _backgroundService;
            set
            {
                if (ReferenceEquals(_backgroundService, value))
                    return;
                _backgroundService = value;
                _messagingCenter?.Send<object, BackgroundServiceState>(this, FromBackgroundMessages.BackgroundServiceState, new BackgroundServiceState(BoundToService && BackgroundService.IsStarted));
            }
        }

        private bool BoundToService => BackgroundService != null;

        private BackgroundServiceConnection _serviceConnection;
        private IMessagingCenter _messagingCenter;
        private NativeBackgroundServiceHost _backgroundService;

        /// <inheritdoc />
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            _serviceConnection = new BackgroundServiceConnection
            {
                MainActivity = this
            };
            
            _messagingCenter = MessagingCenter.Instance;
            _messagingCenter.Subscribe<object>(this, ToBackgroundMessages.StartBackgroundService, OnStartBackgroundServiceMessage);
            _messagingCenter.Subscribe<object>(this, ToBackgroundMessages.StopBackgroundService, OnStopBackgroundServiceMessage);
            _messagingCenter.Subscribe<object>(this, ToBackgroundMessages.GetBackgroundServiceState, OnGetBackgroundServiceState);

            if (AskForBatteryOptimizations)
            {
                BrandPowerManagement.StartPowerSaverIntent(this, Application.PackageName);
                RequestBatteryOptimization();
            }
        }

        private void OnGetBackgroundServiceState(object obj)
        {
            var msg = new BackgroundServiceState(BoundToService && BackgroundService.IsStarted);
            _messagingCenter.Send<object, BackgroundServiceState>(this, FromBackgroundMessages.BackgroundServiceState, msg);
        }

        /// <inheritdoc />
        protected override void OnStart()
        {
            base.OnStart();
            BindService(new Intent(this, typeof(NativeBackgroundServiceHost)), _serviceConnection, Bind.AutoCreate);
            Android.Util.Log.Info(Tag, "Binding to service...");
        }

        /// <inheritdoc />
        protected override void OnStop()
        {
            base.OnStop();
            Android.Util.Log.Info(Tag, "Unbinding service... ");
            UnbindService(_serviceConnection);
        }

        private void OnStartBackgroundServiceMessage(object sender)
        {
            if (BoundToService && BackgroundService.IsStarted)
                return;
            Android.Util.Log.Info(Tag, "Starting service...");
            this.StartForegroundServiceCompat<NativeBackgroundServiceHost>();
        }

        private void OnStopBackgroundServiceMessage(object sender)
        {
            if (!BoundToService || !BackgroundService.IsStarted)
                return;
            Android.Util.Log.Info(Tag, "Stopping service...");
            this.StartForegroundServiceCompat<NativeBackgroundServiceHost>(new Dictionary<string, bool> { { NativeBackgroundServiceHost.StopServiceExtra, true } });
        }

        /// <inheritdoc />
        protected override void OnDestroy()
        {
            _messagingCenter.Unsubscribe<object>(this, ToBackgroundMessages.StartBackgroundService);
            _messagingCenter.Unsubscribe<object>(this, ToBackgroundMessages.StopBackgroundService);
            _messagingCenter.Unsubscribe<object>(this, ToBackgroundMessages.GetBackgroundServiceState);

            base.OnDestroy();
        }

        private void RequestBatteryOptimization()
        {
            using (var powerService = ApplicationContext.GetSystemService(PowerService) as PowerManager)
            {
                if (powerService == null || powerService.IsIgnoringBatteryOptimizations(Application.PackageName))
                    return;
                var intent = new Intent(Settings.ActionIgnoreBatteryOptimizationSettings);
                StartActivity(intent);
            }
        }
    }
}