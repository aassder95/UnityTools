using System.IO;

namespace UnityTools.Util.Core.Persistence
{
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
