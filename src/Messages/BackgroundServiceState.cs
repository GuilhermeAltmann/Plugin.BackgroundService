namespace Plugin.BackgroundService.Messages
{
    /// <summary>
    /// Background service state
    /// </summary>
    internal class BackgroundServiceState
    {
        /// <summary>
        /// True if background service is running
        /// </summary>
        public bool IsRunning { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isRunning"></param>
        public BackgroundServiceState(bool isRunning)
        {
            IsRunning = isRunning;
        }
    }
}