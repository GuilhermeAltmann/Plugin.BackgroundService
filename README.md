# Xamarin.Forms Background Service

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
