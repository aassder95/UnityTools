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
}
