using System.Threading.Tasks;

namespace Plugin.BackgroundService
{
    
    /// <inheritdoc />
    /// <summary>
    /// Same as <see cref="T:Plugin.BackgroundService.IService" /> but provide a periodic task to call
    /// </summary>
    public interface IPeriodicService : IService
    {
        /// <summary>
        /// A periodic task called at the interval defined in <see cref="BackgroundServiceHost.PeriodicServiceCallInterval"/>
        /// </summary>
        /// <returns></returns>
        Task PeriodicActionAsync();
    }
}
