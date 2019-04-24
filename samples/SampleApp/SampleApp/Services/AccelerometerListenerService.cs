using System.Threading.Tasks;
using Plugin.BackgroundService;
using Xamarin.Essentials;

namespace SampleApp.Services
{
    public class AccelerometerListenerService : IService
    {
        public Task StartAsync()
        {
            Accelerometer.Start(SensorSpeed.Default);
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            Accelerometer.Stop();
            return Task.CompletedTask;
        }
    }
}
