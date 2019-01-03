using Android.OS;

namespace Plugin.BackgroundService
{
    /// <summary>
    /// Binder to <see cref="NativeBackgroundServiceHost"/>
    /// </summary>
    public class BackgroundServiceBinder : Binder
    {
        /// <summary>
        /// Reference to the current <see cref="NativeBackgroundServiceHost"/>. Can be null if not running
        /// </summary>
        public NativeBackgroundServiceHost Service { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="service"></param>
        public BackgroundServiceBinder(NativeBackgroundServiceHost service)
        {
            Service = service;
        }
    }
}