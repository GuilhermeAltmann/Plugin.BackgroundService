# Xamarin.Forms Background Service

[![Build status](https://neovigie.visualstudio.com/Plugin.BackgroundService/_apis/build/status/Plugin.BackgroundService-CI)](https://neovigie.visualstudio.com/Plugin.BackgroundService/_build/latest?definitionId=28)

This plugin can be used for creating background services on Android and iOS.

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
NativeBackgroundServiceHost.Init(this,
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

### How to interact with the background service?

In order to start the background service, you have to send a message through the MessagingCenter:
```csharp
var messagingCenter = MessagingCenter.Instance;
messagingCenter.Send<object>(this, Plugin.BackgroundService.Messages.ToBackgroundMessages.StartBackgroundService);
```

For stopping it:
```csharp
var messagingCenter = MessagingCenter.Instance;
messagingCenter.Send<object>(this, Plugin.BackgroundService.Messages.ToBackgroundMessages.StopBackgroundService);
```

Subscribe to background service state:
```csharp
var messagingCenter = MessagingCenter.Instance;
messagingCenter.Subscribe<object, BackgroundServiceState>(this, Plugin.BackgroundService.Messages.FromBackgroundMessages.BackgroundServiceState, OnBackgroundServiceState);
```
Do not forget to unsubscribe from `MessagingCenter`:
```csharp
var messagingCenter = MessagingCenter.Instance;
messagingCenter.Unsubscribe<object, BackgroundServiceState>(this, Plugin.BackgroundService.Messages.FromBackgroundMessages.BackgroundServiceState);
```

Send request for getting background service state:
```csharp
var messagingCenter = MessagingCenter.Instance;
messagingCenter.Send<object>(this, Plugin.BackgroundService.Messages.ToBackgroundMessages.GetBackgroundServiceState);
```

### Notes

Please note that all your `IService` classes MUST NOT use any form of dependency injection, or at least, not the same as your application. The background service and your application do not have the same life cycle and you may have some mixed up references between your background service and your app if you use the same IoC.