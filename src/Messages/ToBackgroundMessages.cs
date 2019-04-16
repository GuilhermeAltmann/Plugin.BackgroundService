namespace Plugin.BackgroundService.Messages
{
    /// <summary>
    /// Messages send to background service
    /// </summary>
    public static class ToBackgroundMessages
    {
        /// <summary>
        /// Start all background services
        /// </summary>
        public const string StartBackgroundService = "StartBackgroundService";

        /// <summary>
        /// Stop all background services
        /// </summary>
        public const string StopBackgroundService = "StopBackgroundService";

        /// <summary>
        /// Get current background service host state
        /// </summary>
        public const string GetBackgroundServiceState = "GetBackgroundServiceState";

        /// <summary>
        /// Update the notification message.
        /// Works only for Android.
        /// </summary>
        public const string UpdateBackgroundServiceNotificationMessage = "UpdateBackgroundServiceNotificationMessage";
    }
}