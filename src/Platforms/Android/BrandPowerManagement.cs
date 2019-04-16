using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Content.PM;

// ReSharper disable once CheckNamespace
namespace Plugin.BackgroundService
{
    /// <summary>
    /// Thank you sir: https://stackoverflow.com/a/49110392/9539955
    /// Handle brands customer power management by asking the user to disable it for the app.
    /// </summary>
    public static class BrandPowerManagement
    {
        private static readonly List<Intent> PowerManagerIntents = new List<Intent>()
        {
            new Intent().SetComponent(new ComponentName("com.miui.securitycenter", "com.miui.permcenter.autostart.AutoStartManagementActivity")),
            new Intent().SetComponent(new ComponentName("com.letv.android.letvsafe", "com.letv.android.letvsafe.AutobootManageActivity")),
            new Intent().SetComponent(new ComponentName("com.huawei.systemmanager", "com.huawei.systemmanager.optimize.process.ProtectActivity")),
            new Intent().SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.permission.startup.StartupAppListActivity")),
            new Intent().SetComponent(new ComponentName("com.coloros.safecenter", "com.coloros.safecenter.startupapp.StartupAppListActivity")),
            new Intent().SetComponent(new ComponentName("com.oppo.safe", "com.oppo.safe.permission.startup.StartupAppListActivity")),
            new Intent().SetComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.AddWhiteListActivity")),
            new Intent().SetComponent(new ComponentName("com.iqoo.secure", "com.iqoo.secure.ui.phoneoptimize.BgStartUpManager")),
            new Intent().SetComponent(new ComponentName("com.vivo.permissionmanager", "com.vivo.permissionmanager.activity.BgStartUpManagerActivity")),
            new Intent().SetComponent(new ComponentName("com.asus.mobilemanager", "com.asus.mobilemanager.entry.FunctionActivity"))
                .SetData(Android.Net.Uri.Parse("mobilemanager://function/entry/AutoStart"))
        };

        /// <summary>
        /// Ask the user to disable the brand power management, open the correct intent to do so
        /// </summary>
        /// <param name="context"></param>
        /// <param name="appName">Name of the current app</param>
        public static void StartPowerSaverIntent(Context context, string appName)
        {
            var settings = context.GetSharedPreferences("ProtectedApps", FileCreationMode.Private);
            var skipMessage = settings.GetBoolean("skipAppListMessage", false);
            if (skipMessage)
                return;
            var editor = settings.Edit();
            foreach (var intent in PowerManagerIntents)
            {
                if (context.PackageManager.ResolveActivity(intent, PackageInfoFlags.MatchDefaultOnly) == null)
                    continue;
                var dontShowAgain = new Android.Support.V7.Widget.AppCompatCheckBox(context)
                {
                    Text = "Do not show again"
                };
                dontShowAgain.CheckedChange += (sender, e) =>
                {
                    editor.PutBoolean("skipAppListMessage", e.IsChecked);
                    editor.Apply();
                };

                new AlertDialog.Builder(context)
                    .SetIcon(Android.Resource.Drawable.IcDialogAlert)
                    .SetTitle($"Add {appName} to list")
                    .SetMessage($"{appName} requires to be enabled/added in the list to function properly.\n")
                    .SetView(dontShowAgain)
                    .SetPositiveButton("Go to settings", (o, d) => context.StartActivity(intent))
                    .SetNegativeButton(Android.Resource.String.Cancel, (o, d) => { })
                    .Show();

                break;
            }
        }
    }
}