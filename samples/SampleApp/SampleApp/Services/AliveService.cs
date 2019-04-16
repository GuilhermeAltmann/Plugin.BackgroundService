using System;
using System.Threading.Tasks;
using Plugin.BackgroundService;
using Prism;
using Prism.Ioc;
using Prism.Unity;

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

        public Task StopAsync()
        {
            Console.WriteLine("Well... I'm dead now.");
            return Task.CompletedTask;
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
