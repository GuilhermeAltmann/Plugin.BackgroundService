using System;

namespace Plugin.BackgroundService
{
    public interface IBackgroundService : IDisposable
    {
        /// <summary>
        /// Get the current running state of the background service
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Start the background service
        /// </summary>
        void StartService();

        /// <summary>
        /// Stop the background service
        /// </summary>
        void StopService();

        /// <summary>
        /// Update the text content of the service notification.
        /// </summary>
        /// <param name="newText">New text to show in notification content</param>
        void UpdateNotificationMessage(string newText);

        /// <summary>
        /// Event raised when the background service state change.
        /// The eventArgs contains the state.
        /// </summary>
        event EventHandler<BackgroundServiceRunningStateChangedEventArgs> BackgroundServiceRunningStateChanged;
    }
}