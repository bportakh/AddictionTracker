using Android.App;
using Android.App.Usage;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AppUsageStatistics
{
    [Service]
    public class SpentTimeCheckingService : Service
    {
        const long HOUR_IN_MILLISECONDS = 3600000;

        UsageStatsManager mUsageStatsManager;

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
                        mUsageStatsManager = (UsageStatsManager)GetSystemService("usagestats");
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
                    CheckSpentTimes();
                }

            });
        }

        private void CheckSpentTimes()
        {
            Log.Debug("SS", "executing");

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

            var appOverflowingLimit = result.FirstOrDefault(x => x.TotalTimeInForeground > HOUR_IN_MILLISECONDS);

            if (appOverflowingLimit != null)
            {
                // Instantiate the builder and set notification elements:
                Notification.Builder builder = new Notification.Builder(this)
                    .SetContentTitle("Shut down app")
                    .SetContentText(appOverflowingLimit.PackageName)
                    .SetSmallIcon(Resource.Drawable.icon);

                // Build the notification:
                Notification notification = builder.Build();

                // Get the notification manager:
                NotificationManager notificationManager =
                    GetSystemService(Context.NotificationService) as NotificationManager;

                // Publish the notification:
                const int notificationId = 0;
                notificationManager.Notify(notificationId, notification);
            }
        }

    }
}