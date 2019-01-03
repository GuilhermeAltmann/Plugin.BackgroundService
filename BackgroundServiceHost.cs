using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Plugin.BackgroundService
{
    /// <summary>
    /// Host of all background services
    /// </summary>
    public class BackgroundServiceHost
    {
        /// <summary>
        /// Define the period between two calls of method <see cref="IPeriodicService.PeriodicActionAsync()"/>
        /// </summary>
        public TimeSpan PeriodicServiceCallInterval { get; }

        private readonly List<IService> _services;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="servicesInitializer">Initialize all background services</param>
        /// <param name="periodicServiceCallInterval">Define the period between two calls of method <see cref="IPeriodicService.PeriodicActionAsync()"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BackgroundServiceHost(Action<List<IService>> servicesInitializer, TimeSpan periodicServiceCallInterval)
        {
            if (servicesInitializer == null) throw new ArgumentNullException(nameof(servicesInitializer));
            if (periodicServiceCallInterval == null) throw new ArgumentNullException(nameof(periodicServiceCallInterval));

            try
            {
                PeriodicServiceCallInterval = periodicServiceCallInterval;
                _services = new List<IService>();
                servicesInitializer(_services);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Start all background services
        /// </summary>
        /// <returns></returns>
        public async Task StartAsync()
        {
            try
            {
                IEnumerable<Task> startServicesTasks;

                Debug.WriteLine("Starting all background services");

                // Start all services here
                lock (_services)
                {
                    startServicesTasks = _services.Select(x => x.StartAsync());
                }

                await Task.WhenAll(startServicesTasks);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Stop all background services
        /// </summary>
        /// <returns></returns>
        public async Task StopAsync()
        {
            try
            {
                IEnumerable<Task> stopServicesTasks;
                Debug.WriteLine("Stopping all background services");

                // Stop all services here
                lock (_services)
                {
                    stopServicesTasks = _services.Select(x => x.StopAsync());
                }

                await Task.WhenAll(stopServicesTasks);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

        /// <summary>
        /// Periodic task for <see cref="IPeriodicService"/>
        /// </summary>
        /// <returns></returns>
        public async Task PeriodicTaskAsync()
        {
            try
            {
                // Start all services here
                IEnumerable<IPeriodicService> periodicServices;
                Debug.WriteLine("Periodic call on all periodic services");

                lock (_services)
                {
                    periodicServices = _services.OfType<IPeriodicService>();
                }
                await Task.WhenAll(periodicServices.Select(x => x.PeriodicActionAsync()));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
