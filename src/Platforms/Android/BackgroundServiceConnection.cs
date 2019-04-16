using Android.Content;
using Android.OS;

// ReSharper disable once CheckNamespace
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
            Android.Util.Log.Info(MainActivity.Tag, "Connecting to service... Handle: [{0}], Hash: [{1}]", binder?.Service.Handle.ToString(), binder?.Service.JniIdentityHashCode);
            MainActivity.BackgroundService = binder?.Service;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void OnServiceDisconnected(ComponentName name)
        {
            Android.Util.Log.Info(MainActivity.Tag, "Disconnecting from service... Handle: [{0}], Hash: [{1}]", MainActivity.BackgroundService.Handle.ToString(), MainActivity.BackgroundService.JniIdentityHashCode);
            MainActivity.BackgroundService = null;
        }
    }
}