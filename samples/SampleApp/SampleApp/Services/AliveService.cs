using System;
using System.Threading.Tasks;
using Plugin.BackgroundService;
using Prism.Ioc;

namespace SampleApp.Services
{
    /// <summary>
    /// Periodic background service
    /// </summary>
    public class AliveService : BackgroundServiceBase, IPeriodicService
    {
        private int _count;
        private IFactService _factService;


        public override async Task StartAsync()
        {
            _count = 0;
            Console.WriteLine("Hey, I'm born! Hello World!");
            await RefreshServicesIfNeededAsync();
        }

        public override async Task StopAsync()
        {
            await RefreshServicesIfNeededAsync();
            var fact = await _factService.GetLatestFactAsync();
            if (fact != null)
            {
                Console.WriteLine("I learned something interesting before dying: {0}", fact.Text);
                Console.WriteLine("I'll take a moment to meditate on that thought...");
                // This delay is here for the sake of repro for issue #3.
                await Task.Delay(TimeSpan.FromSeconds(5));
            }

            Console.WriteLine("Well... I'm dead now.");
        }

        protected override Task InitializeServicesAsync()
        {
            if (!IsApplicationAttached)
                throw new InvalidOperationException("No application attached to background service. Unable to resolve services.");

            _factService = AppContainer.Resolve<IFactService>();

            return Task.CompletedTask;
        }

        protected override Task CleanUpServicesAsync()
        {
            _factService = null;

            return Task.CompletedTask;
        }

        public async Task PeriodicActionAsync()
        {
            Console.WriteLine("I'm still alive! [{0}]", _count++);
            await RefreshServicesIfNeededAsync();
            var fact = await _factService.GetLatestFactAsync();
            if (fact != null)
                Console.WriteLine("I know something interesting: {0}", fact.Text);

            await _factService.UpdateFactAsync();
        }
    }
}