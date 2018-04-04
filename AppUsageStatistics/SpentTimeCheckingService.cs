using Android.App;
using Android.App.Usage;
using Android.Content;
using Android.Content.PM;
using Android.Media;
using Android.OS;
using Android.Util;
using Android.Widget;
using AppUsageStatistics.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppUsageStatistics
{
    [Service]
    public class SpentTimeCheckingService : Service
    {
        ActivityManager activityManager;
        UsageStatsManager mUsageStatsManager;
        DatabaseService databaseService = new DatabaseService();

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            var t = new Java.Lang.Thread(() =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        databaseService.CreateDataBase();
                        activityManager = (ActivityManager)GetSystemService(Context.ActivityService);
                        ExecuteSpentTimehecking().Wait();
                    }
                    catch (Exception exception)
                    {

                    }
                });
            }
            );
            t.Start();
            return StartCommandResult.Sticky;
        }

        public async Task ExecuteSpentTimehecking()
        {
            await Task.Run(async () =>
            {
                while (1 < 2)
                {
                    await Task.Delay(5000);
                    try
                    {
                        CheckSpentTimes();
                    }
                    catch(Exception exception)
                    {

                    }
                }

            });
        }

        private void CheckSpentTimes()
        {
            mUsageStatsManager = (UsageStatsManager)GetSystemService("usagestats");

            var currentDate = System.DateTime.Now;
            var beginDate = System.DateTime.Now;

            var intervalType = UsageStatsInterval.Daily;

            switch (intervalType)
            {
                case UsageStatsInterval.Yearly:
                    beginDate = new DateTime(beginDate.Year, 1, 1);
                    break;
                case UsageStatsInterval.Monthly:
                    beginDate = new DateTime(beginDate.Year, beginDate.Month, 1);
                    break;
                case UsageStatsInterval.Weekly:
                    beginDate = beginDate.StartOfWeek(DayOfWeek.Monday);
                    break;
                case UsageStatsInterval.Daily:
                    beginDate = beginDate.Date;
                    break;
                default:
                    break;
            }

            var beginTime = (long)(beginDate - new DateTime(1970, 1, 1)).TotalMilliseconds;
            var currentTime = (long)(currentDate - new DateTime(1970, 1, 1)).TotalMilliseconds;

            var queryUsageStats = mUsageStatsManager
                .QueryAndAggregateUsageStats(beginTime,
                                      currentTime);

            if (queryUsageStats.Count == 0)
            {
                Log.Info("SS", "The user may not allow the access to apps usage. ");
                Toast.MakeText(this,
                    GetString(Resource.String.explanation_access_to_appusage_is_not_enabled),
                    ToastLength.Long).Show();
            }

            var result = queryUsageStats.Values.ToList();

            var allAppsToCheck = databaseService.SelectTableAppSettings();
            var packageNamesToCheck1 = allAppsToCheck.Select(x => x.PackageName);

            result.RemoveAll(x => !(packageNamesToCheck1.Contains(x.PackageName)));

            foreach (var usageStat in result)
            {
                var currentDbEntry = allAppsToCheck.FirstOrDefault(x => x.PackageName == usageStat.PackageName);

                if (currentDbEntry != null && usageStat.TotalTimeInForeground > currentDbEntry.DailyLimit)
                {
                    if (currentDbEntry.LastTotalTimeInForeground != usageStat.TotalTimeInForeground)
                    {
                        TimeSpan timeSpanExceeded = TimeSpan.FromMilliseconds((double)(usageStat.TotalTimeInForeground - currentDbEntry?.DailyLimit));

                        currentDbEntry.LastTotalTimeInForeground = usageStat.TotalTimeInForeground;

                        var timeMessage = string.Empty;

                        if (timeSpanExceeded.Hours > 0)
                        {
                            timeMessage = $"{timeSpanExceeded.Hours} hours {timeSpanExceeded.Minutes} minutes.";
                        }
                        else
                        {
                            timeMessage = $"{timeSpanExceeded.Minutes} minutes.";

                        }

                        if (currentDbEntry.AppName == null)
                        {
                            currentDbEntry.AppName = Android.App.Application.Context.PackageManager.GetApplicationLabel(PackageManager.GetApplicationInfo(currentDbEntry.PackageName, PackageInfoFlags.MetaData));
                        }


                        Notification.Builder builder = new Notification.Builder(this)
                            .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Notification))
                        .SetContentTitle($"Shut down {currentDbEntry.AppName}")
                        .SetContentText($"Daily limit was exceeded by {timeMessage}")
                        .SetSmallIcon(Resource.Drawable.icon);

                        // Build the notification:
                        Notification notification = builder.Build();

                        // Get the notification manager:
                        NotificationManager notificationManager =
                            GetSystemService(Context.NotificationService) as NotificationManager;

                        // Publish the notification:
                        const int notificationId = 0;
                        notificationManager.Notify(notificationId, notification);

                        databaseService.UpdateTableAppSettings(currentDbEntry);

                    }
                }
            }
        }

        public static List<string> getActiveApps()
        {
            List<ApplicationInfo> packages = Android.App.Application.Context.PackageManager.GetInstalledApplications(PackageInfoFlags.MetaData).ToList();
            List<string> packagesRunning = new List<string>();

            foreach (var packageInfo in packages)
            {
                if (!isSTOPPED(packageInfo))
                {
                    packagesRunning.Add(packageInfo.Name);
                }
            }

            packages.RemoveAll(x => x == null);

            return packagesRunning;
        }

        private static bool isSTOPPED(ApplicationInfo pkgInfo)
        {
            return pkgInfo.Flags.HasFlag(ApplicationInfoFlags.Stopped) == false;
        }

    }
}