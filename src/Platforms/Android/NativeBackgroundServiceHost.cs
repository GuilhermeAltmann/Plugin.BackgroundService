using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Support.V4.App;
using Plugin.BackgroundService.Messages;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

#if __ANDROID__
[assembly: UsesPermission(Manifest.Permission.WakeLock)]
// ReSharper disable once CheckNamespace
namespace Plugin.BackgroundService
{
    /// <summary>
    /// Background service host for Android
    /// </summary>
    [Service(Enabled = true, Exported = false)]
    public class NativeBackgroundServiceHost : Service
    {
        private static string _serviceName;
        private static string _serviceNotificationChannelId;
        private static string _serviceNotificationChannelName;
        private static int _serviceNotificationIcon;
        private static string _serviceNotificationTitle;
        private static string _serviceNotificationContent;
        private static int _serviceNotificationId = new Random().Next();
        private static Func<BackgroundServiceHost> _backgroundServiceCreationFunc;
        private static Type _intentLaunchType;
        private static bool _hasPeriodicTask;

        /// <summary>
        /// Check if service is running
        /// </summary>
        public bool IsStarted { get; private set; }
        /// <summary>
        /// Extra to add to service intent for stopping this service
        /// </summary>
        public const string StopServiceExtra = "StopBackgroundService";

        private string _actionMainActivity;

        private NotificationManager _notificationManager;
        private readonly IMessagingCenter _messagingCenter;
        private readonly IBinder _binder;

        private PowerManager.WakeLock _wakeLock;
        private HandlerThread _handlerThread;
        private Handler _handler;
#pragma warning disable 414
        private bool _configurationChanging;
#pragma warning restore 414
        private BackgroundServiceHost _backgroundService;
        private bool _starting;

        /// <summary>
        /// Initialize the service host. Must be called after <see cref="FormsAppCompatActivity.LoadApplication"/>
        /// </summary>
        /// <param name="serviceName">Name of the service in the system</param>
        /// <param name="serviceNotificationChannelId">Id of the channel dedicated for the service persistent notification</param>
        /// <param name="serviceNotificationChannelName">Name of the channel dedicated for the service persistent notification</param>
        /// <param name="serviceNotificationIcon">Persistent notification icon. MUST BE DEFINED, otherwise the notification do not shows up</param>
        /// <param name="serviceNotificationTitle">Persistent notification title displayed to the user</param>
        /// <param name="serviceNotificationContent">Persistent notification content displayed to the user</param>
        /// <param name="backgroundServiceCreationFunc">Callback for initialization of all <see cref="IService"/> running in background</param>
        /// <param name="intentLaunchType">Type of the intent launcher (generally MainActivity)</param>
        /// <param name="hasPeriodicTask">Define if the background service must run a periodic task</param>
        public static void Init(string serviceName,
            string serviceNotificationChannelId,
            string serviceNotificationChannelName,
            int serviceNotificationIcon,
            string serviceNotificationTitle,
            string serviceNotificationContent,
            Func<BackgroundServiceHost> backgroundServiceCreationFunc,
            Type intentLaunchType = null,
            bool hasPeriodicTask = true)
        {
            _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
            _serviceNotificationChannelId = serviceNotificationChannelId ?? throw new ArgumentNullException(nameof(serviceNotificationChannelId));
            _serviceNotificationChannelName = serviceNotificationChannelName ?? throw new ArgumentNullException(nameof(serviceNotificationChannelName));
            _serviceNotificationIcon = serviceNotificationIcon;
            _serviceNotificationTitle = serviceNotificationTitle ?? throw new ArgumentNullException(nameof(serviceNotificationTitle));
            _serviceNotificationContent = serviceNotificationContent ?? throw new ArgumentNullException(nameof(serviceNotificationContent));
            _backgroundServiceCreationFunc = backgroundServiceCreationFunc ?? throw new ArgumentNullException(nameof(backgroundServiceCreationFunc));
            _intentLaunchType = intentLaunchType;
            _hasPeriodicTask = hasPeriodicTask;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <exception cref="NullReferenceException"></exception>
        public NativeBackgroundServiceHost()
        {
            if (_backgroundServiceCreationFunc == null)
                throw new NullReferenceException("You probably forgot to call Init() method for this class");

            _binder = new BackgroundServiceBinder(this);
            _messagingCenter = MessagingCenter.Instance;
        }

        /// <inheritdoc />
        public override void OnCreate()
        {
            base.OnCreate();
            _actionMainActivity = $"{Application.PackageName}.show_main_activity";

            _notificationManager = GetSystemService(NotificationService) as NotificationManager;
            if (_notificationManager == null)
            {
                Android.Util.Log.Warn(_serviceName, "Unable to get NotificationManager in NativeBackgroundService");
                return;
            }

            // Create notification channel
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var notificationChannel = new NotificationChannel(_serviceNotificationChannelId,
                    _serviceNotificationChannelName, NotificationImportance.Default)
                {
                    LockscreenVisibility = NotificationVisibility.Secret
                };
                _notificationManager.CreateNotificationChannel(notificationChannel);
            }
        }

        /// <inheritdoc />
        public override IBinder OnBind(Intent intent)
        {
            _configurationChanging = false;
            return _binder;
        }

        /// <inheritdoc />
        public override void OnRebind(Intent intent)
        {
            _configurationChanging = false;
            base.OnRebind(intent);
        }

        /// <inheritdoc />
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var stopRequest = intent.GetBooleanExtra(StopServiceExtra, false);

            if (!stopRequest && (IsStarted || _starting)) // Not a stop request, already started or starting
                return StartCommandResult.Sticky;

            if (!stopRequest) // Start request
            {
                _starting = true;
                BuildForegroundService();
                StartInBackground()
                    .ContinueWith(task =>
                    {
                        _starting = false;
                        IsStarted = true;
                        _messagingCenter.Send<object, BackgroundServiceState>(this,
                            FromBackgroundMessages.BackgroundServiceState, new BackgroundServiceState(true));
                        _messagingCenter.Subscribe<object, UpdateNotificationMessage>(this, 
                            ToBackgroundMessages.UpdateBackgroundServiceNotificationMessage, 
                            OnNotificationMessageUpdate);
                    });
            }
            else // Stop request
            {
                Cleanup().Wait();
            }

            return StartCommandResult.Sticky;
        }

        private void OnNotificationMessageUpdate(object sender, UpdateNotificationMessage message)
        {
            UpdateServiceNotificationContent(message.NewText);
        }

        private void UpdateServiceNotificationContent(string contentText)
        {
            var notification = BuildNotification(contentText);
            _notificationManager.Notify(_serviceNotificationId, notification);
        }

        /// <inheritdoc />
        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            _configurationChanging = true;
        }

        /// <inheritdoc />
        public override void OnDestroy()
        {
            if (IsStarted)
            {
                Cleanup().Wait();
            }

            base.OnDestroy();
        }

        private async Task StartInBackground()
        {
            try
            {
                _backgroundService = _backgroundServiceCreationFunc();
                await _backgroundService.StartAsync();

                using (var powerService = ApplicationContext.GetSystemService(PowerService) as PowerManager)
                {
                    if (powerService == null)
                        return;
                    _wakeLock = powerService.NewWakeLock(WakeLockFlags.Partial, _serviceName);
                }

                AcquireWakelock();
                if (_hasPeriodicTask)
                    StartHandlerThread();
            }
            catch (Exception e)
            {
                Android.Util.Log.Error(_serviceName, e.ToString());
            }
        }

        private Task Cleanup()
        {
            return _backgroundService?.StopAsync()
                .ContinueWith(task =>
                {
                    if (_hasPeriodicTask)
                        StopHandlerThread();
                    ReleaseWakeLock();
                    StopForeground(true);
                    StopSelf();
                    IsStarted = false;
                    _messagingCenter.Send<object, BackgroundServiceState>(this,
                        FromBackgroundMessages.BackgroundServiceState, new BackgroundServiceState(false));
                    _messagingCenter.Unsubscribe<object, UpdateNotificationMessage>(this, ToBackgroundMessages.UpdateBackgroundServiceNotificationMessage);
                });
        }

        private void BuildForegroundService()
        {
            var notification = BuildNotification(_serviceNotificationContent);

            StartForeground(_serviceNotificationId, notification);
        }

        private PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, _intentLaunchType);
            notificationIntent.SetAction(_actionMainActivity);
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            var pendingIntent =
                PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }

        private Notification BuildNotification(string contentText)
        {
            var notificationBuilder = new NotificationCompat.Builder(this, _serviceNotificationChannelId)
                .SetContentTitle(_serviceNotificationTitle)
                .SetSmallIcon(_serviceNotificationIcon)
                .SetContentText(contentText)
                .SetOngoing(true)
                .SetCategory(Notification.CategoryService);

            if (_intentLaunchType != null)
                notificationBuilder.SetContentIntent(BuildIntentToShowMainActivity());

            var notification = notificationBuilder.Build();
            return notification;
        }

        private void AcquireWakelock()
        {
            if (!_wakeLock.IsHeld)
                _wakeLock.Acquire();
        }

        private void ReleaseWakeLock()
        {
            if (_wakeLock != null && _wakeLock.IsHeld)
                _wakeLock.Release();
        }

        private void StopHandlerThread()
        {
            _handler?.RemoveCallbacks(ScheduleNext);
            _handlerThread?.Quit();
            _handlerThread?.Interrupt();
            _handlerThread = null;
        }

        private void StartHandlerThread()
        {
            if (_handlerThread != null)
                StopHandlerThread();
            _handlerThread = new HandlerThread("PeriodicServiceThread", (int) ThreadPriority.Background);
            _handlerThread.Start();
            _handler = new Handler(_handlerThread.Looper);
            ScheduleNext();
        }

        private void ScheduleNext()
        {
            try
            {
                Task.Run(async () => await _backgroundService.PeriodicTaskAsync()).Wait();
            }
            catch (Exception e)
            {
                Android.Util.Log.Error(_serviceName, e.ToString());
            }

            _handler?.PostDelayed(ScheduleNext,
                (long) _backgroundService.PeriodicServiceCallInterval.TotalMilliseconds);
        }
    }
}
#endif