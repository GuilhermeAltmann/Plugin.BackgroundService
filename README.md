# Xamarin.Forms Background Service

| Version | Status |
| --- | --- |
| master | [![Azure DevOps builds](https://img.shields.io/azure-devops/build/nicolas-garcia/Plugin.BackgroundService/2.svg)](https://dev.azure.com/nicolas-garcia/Plugin.BackgroundService/_build?definitionId=2) [![Nuget](https://img.shields.io/nuget/v/Plugin.BackgroundService.svg)](https://www.nuget.org/packages/Plugin.BackgroundService/) |
| dev | [![Azure DevOps builds](https://img.shields.io/azure-devops/build/nicolas-garcia/Plugin.BackgroundService/4.svg)](https://dev.azure.com/nicolas-garcia/Plugin.BackgroundService/_build?definitionId=4) [![Nuget](https://img.shields.io/nuget/vpre/Plugin.BackgroundService.svg)](https://www.nuget.org/packages/Plugin.BackgroundService/) |



This plugin can be used for creating background services on Android and iOS. 

### How it works?

Creating a real background service on a mobile app is real challenge for new mobile developpers. I struggled many times trying to do it the right way. I think I've come up with a solution that works. Running a background service isn't that hard ok. But what if I want that my service execute a bunch of code periodically ? I mean, strictly periodically. I don't want to wait for the user to unlock the screen or whatever. 

On iOS, the solution is quite easy, a loop should do the trick, until the system decide that your service should die. In order to prevent that, you must listen the user location. Of course, if you don't need it in your application, it will be impossible to publish your app on the store. Yeah, my solution isn't magic, sorry.

On Android then, that one was hard, especially with the latest Android releases. I must use an Android Service of course, then use a wakelock for my service and finally use a Handler for scheduling periodic calls precisely.

Anyway, it appears to work, and I use this plugin at work for my developments. So feel free to use it and improve it!

### Android initialization

First, your `MainActivity.cs` must inherit from `Plugin.BackgroundService.ActivityWithBackgroundService`.  
You can override the default behavior of asking for battery optimizations by overriding property `ActivityWithBackgroundService.AskForBatteryOptimizations`.  
e.g:
```csharp
protected override bool AskForBatteryOptimizations
{
    get
    {
    #if DEBUG
        return false;
    #else
        return true;
    #endif
    }
}
```
In this case, the application won't ask for battery optimization if running in debug.

Why I need this?

Because by default, all applications are 'optimized' by Android. It means that you will be suspended when the system decide to. In order to prevent that behavior, the user must disable the optimization of the application. If set to true, this boolean leads to showing a popup to the user before redirecting him to the settings for disabling the optimization.

Then, given your default `OnCreate` method in your `MainActivity.cs`:
```csharp
protected override void OnCreate(Bundle bundle)
{
    TabLayoutResource = Resource.Layout.Tabbar;
    ToolbarResource = Resource.Layout.Toolbar;

    base.OnCreate(bundle);

    Xamarin.Forms.Forms.Init(this, bundle);
    var app = new App(new AndroidInitializer());
    LoadApplication(app);
}
```

Add the following line after `LoadApplication(app)`:
```csharp
NativeBackgroundServiceHost.Init(
    "YOUR_SERVICE_NAME",
    $"{Application.PackageName}.YOUR_SERVICE_NAME", 
    "Your display service name",
    Resource.Drawable.shield, // replace with your own icon. This is mandatory, otherwise the notification is not displayed correctly
    "SERVICE NOTIFICATION TITLE",
    "SERVICE NOTIFICATION CONTENT", 
    () => new BackgroundServiceHost(list => 
    {
        list.Add(AServiceYouWantToKeepBackground); // Should inherit from IService or IPeriodicService
    }, TimeSpan.FromSeconds(5)), // For periodic services, you can set the interval between calls
    typeof(MainActivity));
```

Ok, that's it for Android. Now, you have a persistent background service. You can kill your app, it will remains in background.

### iOS

First, your `AppDelegate` must inherit from `AppDelegateWithBackgroundService`.  
In `FinishedLaunching` method, add the following line __BEFORE__ `base.FinishedLaunching(app, options)` call:

```csharp
NativeBackgroundServiceHost.Init(() => new BackgroundServiceHost(list => 
{ 
    list.Add(AServiceYouWantToKeepBackground); // Should inherit from IService or IPeriodicService
    ... etc
},
 TimeSpan.FromSeconds(5)) // For periodic services, you can set the interval between calls
);

```

In order to keep the app running, your app must be declared as [supporting background tasks](https://developer.apple.com/library/archive/documentation/iPhone/Conceptual/iPhoneOSProgrammingGuide/BackgroundExecution/BackgroundExecution.html).

Annnnd you're done. Unlike Android, the background service can run only if you are listening geolocation for example, and while you're listening to it. If you kill the app, the background service will be stopped too.

### Notes

Please note that all your `IService` classes MUST NOT use any form of dependency injection, or at least, not the same as your application. The background service and your application do not have the same life cycle and you may have some mixed up references between your background service and your app if you use the same IoC.

### How to interact with the background service?

In order to start the background service:
```csharp
CrossBackgroundService.Current.StartService();
```

For stopping it:
```csharp
CrossBackgroundService.Current.StopService();
```

Subscribe to background service state:
```csharp
CrossBackgroundService.Current.BackgroundServiceRunningStateChanged += (s, e) => Console.WriteLine(e.IsRunning);
```
Do not forget to unsubscribe from this event.

Getting background service state:
```csharp
CrossBackgroundService.Current.IsRunning
```

Updating the text content of the service notification:
```csharp
CrossBackgroundService.Current.UpdateNotificationMessage("my new text");
```
