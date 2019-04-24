using System;
using System.Threading.Tasks;
using Plugin.BackgroundService;
using Prism;
using Prism.Ioc;

namespace SampleApp.Services
{
    /// <summary>
    /// Periodic background service
    /// </summary>
    public class AliveService : IPeriodicService
    {
        private int _count;
        private IFactService _factService;

        public AliveService()
        {
        }

        public Task StartAsync()
        {
            _count = 0;
            Console.WriteLine("Hey, I'm born! Hello World!");
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (PrismApplicationBase.Current != null && _factService == null)
                _factService = PrismApplicationBase.Current.Container.Resolve<IFactService>();
            if (_factService != null)
            {
                var fact = await _factService.GetLatestFactAsync();
                if (fact != null)
                {
                    Console.WriteLine("I learned something interesting before dying: {0}", fact.Text);
                    Console.WriteLine("I'll take a moment to meditate on that thought...");
                    // This delay is here for the sake of repro for issue #3.
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }

            Console.WriteLine("Well... I'm dead now.");
        }

        public async Task PeriodicActionAsync()
        {
            Console.WriteLine("I'm still alive! [{0}]", _count++);

            if (PrismApplicationBase.Current != null && _factService == null)
                _factService = PrismApplicationBase.Current.Container.Resolve<IFactService>();
            if (_factService != null)
            {
                var fact = await _factService.GetLatestFactAsync();
                if (fact != null)
                {
                    Console.WriteLine("I know something interesting: {0}", fact.Text);
                }

                await _factService.UpdateFactAsync();
            }
        }
    }
}