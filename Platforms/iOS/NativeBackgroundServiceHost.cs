using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Plugin.BackgroundService.Messages;
//using CoreLocation;
//using UIKit;
using Xamarin.Forms;

namespace Plugin.BackgroundService
{
#if XAMARIN_IOS
    /// <summary>
    /// Native background service host for iOS
    /// </summary>
    public class NativeBackgroundServiceHost
    {
        /// <summary>
        /// True if background services are running, else False
        /// </summary>
        public bool IsStarted { get; private set; }

        private static Func<BackgroundServiceHost> _backgroundServiceCreationFunc;

        //private readonly CLLocationManager _locationManager;
        private readonly IMessagingCenter _messagingCenter;

        private CancellationTokenSource _cancellationTokenSource;
        private BackgroundServiceHost _backgroundService;

        /// <summary>
        /// Initialize some properties. MUST be called before constructor.
        /// </summary>
        /// <param name="backgroundServiceCreationFunc">Function for all background services initialization</param>
        public static void Init(Func<BackgroundServiceHost> backgroundServiceCreationFunc)
        {
            _backgroundServiceCreationFunc = backgroundServiceCreationFunc ?? throw new ArgumentNullException(nameof(backgroundServiceCreationFunc));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public NativeBackgroundServiceHost()
        {
            _messagingCenter = MessagingCenter.Instance;
            if (_backgroundServiceCreationFunc == null)
                throw new InvalidOperationException("You must call NativeBackgroundServiceHost.Init() before instantiate it");

            //_locationManager = new CLLocationManager
            //{
            //    PausesLocationUpdatesAutomatically = false
            //};

            //if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            //{
            //    _locationManager.RequestAlwaysAuthorization();
            //}

            //if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            //    _locationManager.AllowsBackgroundLocationUpdates = true;

            //_locationManager.DesiredAccuracy = 1;
            //_locationManager.ActivityType = CLActivityType.Other;
            //_locationManager.ShowsBackgroundLocationIndicator = true;

        }

        /// <summary>
        /// Start the native background service host
        /// </summary>
        public void Start()
        {
            if (/*!CLLocationManager.LocationServicesEnabled || */IsStarted)
                return;
            IsStarted = true;
            _cancellationTokenSource = new CancellationTokenSource();

            //_locationManager.LocationsUpdated += OnLocationsUpdated;
            //_locationManager.LocationUpdatesResumed += OnLocationUpdatesResumed;
            //_locationManager.LocationUpdatesPaused += OnLocationUpdatesPaused;
            //_locationManager.StartUpdatingLocation();
            Task.Factory.StartNew(async (obj) => await BackgroundTaskAsync(),
                TaskCreationOptions.LongRunning,
                _cancellationTokenSource.Token);
        }

        private async Task BackgroundTaskAsync()
        {
            try
            {
                _backgroundService = _backgroundServiceCreationFunc();
                await _backgroundService.StartAsync();
                _messagingCenter.Send<object, BackgroundServiceState>(this,
                    FromBackgroundMessages.BackgroundServiceState, new BackgroundServiceState(true));
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        await _backgroundService.PeriodicTaskAsync();
                        if (!_cancellationTokenSource.IsCancellationRequested)
                            await Task.Delay(_backgroundService.PeriodicServiceCallInterval, _cancellationTokenSource.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

        }

        /// <summary>
        /// Stop the native background service host
        /// </summary>
        public void Stop()
        {
            if (!IsStarted)
                return;
            IsStarted = false;
            //_locationManager.StopUpdatingLocation();
            //_locationManager.LocationsUpdated -= OnLocationsUpdated;
            //_locationManager.LocationUpdatesResumed -= OnLocationUpdatesResumed;
            //_locationManager.LocationUpdatesPaused -= OnLocationUpdatesPaused;
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _backgroundService.StopAsync()
                .ContinueWith(task => _messagingCenter.Send<object, BackgroundServiceState>(this, FromBackgroundMessages.BackgroundServiceState, new BackgroundServiceState(false)));
        }

        //private void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        //{
        //}

        //private void OnLocationUpdatesPaused(object sender, EventArgs e)
        //{
        //}

        //private void OnLocationUpdatesResumed(object sender, EventArgs e)
        //{
        //}
    }
#endif
}