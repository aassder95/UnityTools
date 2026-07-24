using System;
using System.IO;

namespace UnityTools.Util.Core.Persistence
{
    public class FileStorage : IStorage
    {
        //============================================================
        // Readonly
        //============================================================
        private readonly string _rootPath;
        private readonly string _rootPrefix;

        //============================================================
        // Constructors
        //============================================================
        private FileStorage(string rootPath)
        {
            _rootPath = rootPath;
            _rootPrefix = rootPath.EndsWith(Path.DirectorySeparatorChar.ToString()) ? rootPath : rootPath + Path.DirectorySeparatorChar;
        }

        //============================================================
        // Init/Register
        //============================================================
        public static bool TryCreate(string rootPath, out FileStorage storage)
        {
            storage = null;
            if (string.IsNullOrWhiteSpace(rootPath))
                return false;

            try
            {
                string normalizedRootPath = Path.GetFullPath(rootPath);
                if (!Directory.Exists(normalizedRootPath))
                    Directory.CreateDirectory(normalizedRootPath);

                storage = new FileStorage(normalizedRootPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //============================================================
        // Persistence
        //============================================================
        public bool TrySave(string key, string data)
        {
            if (data == null || !TryGetPath(key, out string path))
                return false;

            try
            {
                File.WriteAllText(path, data);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryLoad(string key, out string data)
        {
            data = null;
            if (!TryGetPath(key, out string path))
                return false;

            try
            {
                if (!File.Exists(path))
                    return false;

                data = File.ReadAllText(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryHasKey(string key, out bool hasKey)
        {
            hasKey = false;
            if (!TryGetPath(key, out string path))
                return false;

            try
            {
                hasKey = File.Exists(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryDelete(string key)
        {
            if (!TryGetPath(key, out string path))
                return false;

            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        //============================================================
        // Utilities
        //============================================================
        private bool TryGetPath(string key, out string path)
        {
            path = null;
            if (string.IsNullOrWhiteSpace(key) || Path.IsPathRooted(key) || key == "." || key == ".." || key.IndexOf(Path.DirectorySeparatorChar) >= 0 || key.IndexOf(Path.AltDirectorySeparatorChar) >= 0 || key.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;

            try
            {
                string candidatePath = Path.GetFullPath(Path.Combine(_rootPath, key));
                StringComparison comparison = Path.DirectorySeparatorChar == '\\' ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                if (!candidatePath.StartsWith(_rootPrefix, comparison))
                    return false;

                path = candidatePath;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
