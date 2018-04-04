using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace AppUsageStatistics
{
    [Activity(Label = "DetailedAppInfoActivity")]
    public class DetailedAppInfoActivity : Activity
    {
        TimePicker timePicker;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.detailed_app_info_activity);
            var packageName = Intent.GetStringExtra("packageName");

            var appIcon = PackageManager
                        .GetApplicationIcon(packageName);
            var appName = PackageManager.GetApplicationLabel(PackageManager.GetApplicationInfo(packageName, PackageInfoFlags.MetaData));

            var appIconView = FindViewById<ImageView>(Resource.Id.detailed_app_icon);
            var appNameView = FindViewById<TextView>(Resource.Id.app_name);

            appIconView.SetImageDrawable(appIcon);
            appNameView.Text = appName;

            timePicker = FindViewById<TimePicker>(Resource.Id.time_picker);
            var saveButton = FindViewById<Button>(Resource.Id.save_changes);
            saveButton.Click += (s, e) =>
            {
                var time = getTime();
            };
            timePicker.SetIs24HourView(Java.Lang.Boolean.True);
            timePicker.CurrentHour = (Java.Lang.Integer)0;
            timePicker.CurrentMinute = (Java.Lang.Integer)0;
        }

        private string getTime()
        {
            return $"{timePicker.CurrentHour} hours : {timePicker.CurrentMinute} min";
        }
    }
}