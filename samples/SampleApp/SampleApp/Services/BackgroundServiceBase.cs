using System;
using System.Threading.Tasks;
using Plugin.BackgroundService;
using Prism;
using Prism.Ioc;

namespace SampleApp.Services
{
    public abstract class BackgroundServiceBase : IService
    {
        protected static bool IsApplicationAttached => PrismApplicationBase.Current != null;
        protected static IContainerProvider AppContainer => IsApplicationAttached ? PrismApplicationBase.Current.Container : null;

        private static Guid? CurrentApplicationId => IsApplicationAttached ? PrismApplicationBase.Current.Id : (Guid?)null;
        private Guid? _latestApplicationAttached;

        /// <summary>
        /// Checks if an application is currently attached to service. If so, checks if it is the same
        /// application previously used for resolving services. If not, we cleanup current services references
        /// and resolve new ones using the new application
        /// </summary>
        protected async Task RefreshServicesIfNeededAsync()
        {
            if (!IsApplicationAttached)
            {
                Console.WriteLine("No application attached to service, we cannot refresh services");
                return;
            }

            if (_latestApplicationAttached == null && CurrentApplicationId != null)
            {
                Console.WriteLine("No previous application attached, we will initialize services using the current one");
                _latestApplicationAttached = CurrentApplicationId;
                await InitializeServicesAsync();
            }
            else if (_latestApplicationAttached != null && _latestApplicationAttached != CurrentApplicationId)
            {
                Console.WriteLine("Attached application changed, we replace services with current application services");
                await CleanUpServicesAsync();
                _latestApplicationAttached = CurrentApplicationId;
                await InitializeServicesAsync();
            }
            else
                Console.WriteLine("No services refresh needed");
        }

        public abstract Task StartAsync();
        public abstract Task StopAsync();

        /// <summary>
        /// Resolve all injected services
        /// Rebind all events on those services if needed
        /// </summary>
        /// <returns></returns>
        protected abstract Task InitializeServicesAsync();

        /// <summary>
        /// Unbind all events from services
        /// Remove services references
        /// </summary>
        /// <returns></returns>
        protected abstract Task CleanUpServicesAsync();
    }
}
