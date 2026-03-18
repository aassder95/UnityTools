using System;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace UnityTools.Util
{
    // Exception: type-centric file uses Types section.
    //============================================================
    //Types
    //============================================================
    public interface IStorage
    {
        //============================================================
        //Persistence
        //============================================================
        void Save(string key, string data);
        string Load(string key);
        bool HasKey(string key);
        void Delete(string key);
    }

    public class PlayerPrefsStorage : IStorage
    {
        //============================================================
        //Persistence
        //============================================================
        public void Save(string key, string data)
        {
            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        public string Load(string key)
        {
            if(string.IsNullOrEmpty(key) || !PlayerPrefs.HasKey(key))
                return string.Empty;

            return PlayerPrefs.GetString(key);
        }
        
        public bool HasKey(string key)
        {
            if(string.IsNullOrEmpty(key))
                return false;

            return PlayerPrefs.HasKey(key);
        }

        public void Delete(string key)
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }
    }

    public class FileStorage : IStorage
    {
        //============================================================
        //Readonly
        //============================================================
        private readonly string _rootPath;

        //============================================================
        //Constructors
        //============================================================
        public FileStorage(string rootPath)
        {
            _rootPath = rootPath;
            if(!Directory.Exists(_rootPath))
                Directory.CreateDirectory(_rootPath);
        }

        //============================================================
        //Persistence
        //============================================================
        public void Save(string key, string data)
        {
            string path = Path.Combine(_rootPath, key);
            File.WriteAllText(path, data);
        }

        public string Load(string key)
        {
            string path = Path.Combine(_rootPath, key);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }

        public bool HasKey(string key)
        {
            string path = Path.Combine(_rootPath, key);
            return File.Exists(path);
        }

        public void Delete(string key)
        {
            string path = Path.Combine(_rootPath, key);
            if(File.Exists(path))
                File.Delete(path);
        }
    }

    public static class StorageValueUtils
    {
        //============================================================
        //Persistence
        //============================================================
        public static void SaveString(IStorage storage, string key, string value)
        {
            if(storage == null || string.IsNullOrEmpty(key))
                return;

            storage.Save(key, value ?? string.Empty);
        }

        public static bool HasKey(IStorage storage, string key)
        {
            if(storage == null || string.IsNullOrEmpty(key))
                return false;

            return storage.HasKey(key);
        }

        public static string LoadString(IStorage storage, string key)
        {
            if(!HasKey(storage, key))
                return string.Empty;

            return storage.Load(key) ?? string.Empty;
        }

        //============================================================
        //Utilities
        //============================================================
        public static DateTime TryLoadDate(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            if(!long.TryParse(raw, out long ticks))
                return DateTime.MinValue;

            if(ticks == DateTime.MinValue.Ticks)
                return DateTime.MinValue;
            if(ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks)
                return DateTime.MinValue;

            return new DateTime(ticks, DateTimeKind.Utc);
        }

        public static double TryLoadDouble(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            bool isSuccess = double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ||
                             double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value);
            if(!isSuccess || double.IsNaN(value) || double.IsInfinity(value))
                return 0d;

            return Math.Max(0d, value);
        }

        public static int TryLoadInt(IStorage storage, string key)
        {
            string raw = LoadString(storage, key);
            if(!int.TryParse(raw, out int value))
                return 0;

            return value;
        }
    }
}
