using Android.Content;
using Android.OS;

namespace Plugin.BackgroundService
{
    /// <summary>
    /// Connection to service for <see cref="ActivityWithBackgroundService"/>
    /// </summary>
    public class BackgroundServiceConnection : Java.Lang.Object, IServiceConnection
    {
        /// <summary>
        /// Reference to current main activity
        /// </summary>
        public ActivityWithBackgroundService MainActivity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="service"></param>
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            var binder = service as BackgroundServiceBinder;
            MainActivity.BackgroundService = binder?.Service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void OnServiceDisconnected(ComponentName name)
        {
            MainActivity.BackgroundService = null;
        }
    }
}