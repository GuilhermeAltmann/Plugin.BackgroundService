using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;

// ReSharper disable once CheckNamespace
namespace Plugin.BackgroundService
{
    /// <summary>
    /// Extensions for <see cref="Context"/>
    /// </summary>
    public static class ContextExtensions
    {
        /// <summary>
        /// Start a foreground service using the best way according to the current api level
        /// </summary>
        /// <param name="context"></param>
        /// <param name="extras">A bunch of extra you may want to pass to the service intent</param>
        /// <typeparam name="T">ServiceType to start</typeparam>
        public static void StartForegroundServiceCompat<T>(this Context context, Dictionary<string, bool> extras = null) where T : Service
        {
            var intent = new Intent(context, typeof(T));
            if (extras != null)
            {
                foreach (var extra in extras)
                    intent.PutExtra(extra.Key, extra.Value);
            }
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
        }
    }
}