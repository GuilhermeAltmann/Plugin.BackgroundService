using System.Threading.Tasks;

namespace Plugin.BackgroundService
{
    /// <summary>
    /// A generic background service
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Start the background service
        /// </summary>
        /// <returns></returns>
        Task StartAsync();

        /// <summary>
        /// Stop the background service
        /// </summary>
        /// <returns></returns>
        Task StopAsync();
    }
}
