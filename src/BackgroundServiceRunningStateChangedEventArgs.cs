using System;

namespace Plugin.BackgroundService
{
    public class BackgroundServiceRunningStateChangedEventArgs : EventArgs
    {
        public bool IsRunning { get; }

        public BackgroundServiceRunningStateChangedEventArgs(bool isRunning)
        {
            IsRunning = isRunning;
        }

    }
}