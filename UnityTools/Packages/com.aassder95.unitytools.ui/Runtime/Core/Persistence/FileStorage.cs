using System;
using System.IO;
using UnityTools.Util.Core.Logging;

namespace UnityTools.Util.Core.Persistence
{
    public class FileStorage : IStorage
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _rootPath;

        //============================================================
        // Constructors
        //============================================================
        private FileStorage(string rootPath)
        {
            _rootPath = rootPath;
        }

        //============================================================
        // Init/Register
        //============================================================
        public static FileStorage Create(string rootPath)
        {
            if(string.IsNullOrWhiteSpace(rootPath))
            {
                DebugLogger.LogError("FileStorage Root Path가 비어 있습니다.");
                return null;
            }

            try
            {
                if(!Directory.Exists(rootPath))
                    Directory.CreateDirectory(rootPath);

                return new FileStorage(rootPath);
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("FileStorage Root 경로를 준비하지 못했습니다. 경로=" + rootPath + ", 원인=" + exception.Message);
                return null;
            }
        }

        //============================================================
        // Persistence
        //============================================================
        public void Save(string key, string data)
        {
            if(!CanUseKey(key) || data == null)
            {
                if(data == null)
                    DebugLogger.LogError("FileStorage에 저장할 Data가 비어 있습니다.");

                return;
            }

            try
            {
                string path = Path.Combine(_rootPath, key);
                File.WriteAllText(path, data);
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("FileStorage 저장에 실패했습니다. Root=" + _rootPath + ", Key=" + key + ", 원인=" + exception.Message);
            }
        }

        public string Load(string key)
        {
            if(!CanUseKey(key))
                return null;

            try
            {
                string path = Path.Combine(_rootPath, key);
                return File.Exists(path) ? File.ReadAllText(path) : null;
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("FileStorage 로드에 실패했습니다. Root=" + _rootPath + ", Key=" + key + ", 원인=" + exception.Message);
                return null;
            }
        }

        public bool HasKey(string key)
        {
            if(!CanUseKey(key))
                return false;

            try
            {
                string path = Path.Combine(_rootPath, key);
                return File.Exists(path);
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("FileStorage 조회에 실패했습니다. Root=" + _rootPath + ", Key=" + key + ", 원인=" + exception.Message);
                return false;
            }
        }

        public void Delete(string key)
        {
            if(!CanUseKey(key))
                return;

            try
            {
                string path = Path.Combine(_rootPath, key);
                if(File.Exists(path))
                    File.Delete(path);
            }
            catch(Exception exception)
            {
                DebugLogger.LogError("FileStorage 삭제에 실패했습니다. Root=" + _rootPath + ", Key=" + key + ", 원인=" + exception.Message);
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private static bool CanUseKey(string key)
        {
            if(!string.IsNullOrWhiteSpace(key))
                return true;

            DebugLogger.LogError("FileStorage Key가 비어 있습니다.");
            return false;
        }
    }
}
