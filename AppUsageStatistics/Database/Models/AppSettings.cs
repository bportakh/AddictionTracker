using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SQLite;

namespace AppUsageStatistics.Database.Models
{
    public class AppSettings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string PackageName { get; set; }
        public string AppName { get; set; }
        public long DailyLimit { get; set; }
        public long LastTotalTimeInForeground { get; set; }
    }
}