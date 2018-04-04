using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AppUsageStatistics.Database.Models;
using SQLite;

namespace AppUsageStatistics.Database
{
    public class DatabaseService
    {
        string folder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

        public bool CreateDataBase()
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "AddictionTracker.db")))
                {
                    connection.CreateTable<AppSettings>();
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }

        public bool InsertIntoTableAppSettings(AppSettings appSettings)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "AddictionTracker.db")))
                {
                    connection.Insert(appSettings);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }

        public List<AppSettings> SelectTableAppSettings()
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "AddictionTracker.db")))
                {
                    return connection.Table<AppSettings>().ToList();

                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return null;
            }
        }

        public bool UpdateTableAppSettings(AppSettings appSettings)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "AddictionTracker.db")))
                {
                    connection.Query<AppSettings>("UPDATE AppSettings set DailyLimit=?,AppName=?,LastTotalTimeInForeground=? Where Id=?", appSettings.DailyLimit, appSettings.AppName,appSettings.LastTotalTimeInForeground, appSettings.Id);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }

        public bool DeleteTableAppSettings(AppSettings appSettings)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "AddictionTracker.db")))
                {
                    connection.Delete(appSettings);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }

        public bool SelectQueryTableAppSettings(int Id)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "AddictionTracker.db")))
                {
                    connection.Query<AppSettings>("SELECT * FROM AppSettings Where Id=?", Id);
                    return true;
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return false;
            }
        }

        public AppSettings SelectQueryTableAppSettings(string packageName)
        {
            try
            {
                using (var connection = new SQLiteConnection(System.IO.Path.Combine(folder, "AddictionTracker.db")))
                {
                    return connection.Query<AppSettings>("SELECT * FROM AppSettings Where PackageName=?", packageName).FirstOrDefault();
                }
            }
            catch (SQLiteException ex)
            {
                Log.Info("SQLiteEx", ex.Message);
                return null;
            }
        }
    }
}