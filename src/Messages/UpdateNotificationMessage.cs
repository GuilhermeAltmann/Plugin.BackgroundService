namespace Plugin.BackgroundService.Messages
{
    /// <summary>
    /// Update notification message
    /// </summary>
    internal class UpdateNotificationMessage
    {
        /// <summary>
        /// New text to display in notification
        /// </summary>
        public string NewText { get; }

        public UpdateNotificationMessage(string newText)
        {
            NewText = newText;
        }

    }
}
