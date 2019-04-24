using System;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Plugin.BackgroundService;
using Prism;
using Prism.Ioc;
using SampleApp.Services;

namespace SampleApp.Droid
{
    [Activity(Label = "SampleApp", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : ActivityWithBackgroundService
    {
        protected override bool AskForBatteryOptimizations => false;

        protected override void OnCreate(Bundle bundle)
        {
            AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidEnvironmentOnUnhandledExceptionRaiser;
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App(new AndroidInitializer()));
            NativeBackgroundServiceHost.Init(
                "YOUR_SERVICE_NAME",
                $"{Application.PackageName}.YOUR_SERVICE_NAME",
                "Your display service name",
                Resource.Drawable.ic_media_play_light, // replace with your own icon. This is mandatory, otherwise the notification is not displayed correctly
                "SERVICE NOTIFICATION TITLE",
                "SERVICE NOTIFICATION CONTENT",
                () => new BackgroundServiceHost(list =>
                {
                    list.Add(new AliveService()); // Should inherit from IService or IPeriodicService
                    list.Add(new AccelerometerListenerService());
                }, TimeSpan.FromSeconds(5)), // For periodic services, you can set the interval between calls
                typeof(MainActivity));
        }

        private void OnAndroidEnvironmentOnUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            Android.Util.Log.Error(Tag, e.Exception.ToString());
        }
    }

    public class AndroidInitializer : IPlatformInitializer
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // Register any platform specific implementations
        }
    }
}

