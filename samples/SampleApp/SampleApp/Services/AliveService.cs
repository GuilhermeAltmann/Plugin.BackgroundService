using System;
using System.Threading.Tasks;
using Plugin.BackgroundService;

namespace SampleApp.Services
{
    public class AliveService : IPeriodicService
    {
        private int _count;

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

        public Task PeriodicActionAsync()
        {
            Console.WriteLine("I'm still alive! [{0}]", _count++);
            return Task.CompletedTask;
        }
    }
}
