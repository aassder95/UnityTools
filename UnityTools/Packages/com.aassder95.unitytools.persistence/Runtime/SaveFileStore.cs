using System;
using System.IO;
using System.Text;

namespace UnityTools.Persistence
{
    public class SaveFileStore
    {
        //============================================================
        // Fields
        //============================================================
        private readonly string _path;
        private readonly string _backupPath;

        //============================================================
        // Constructors
        //============================================================
        public SaveFileStore(string path)
        {
            _path = path;
            _backupPath = path + ".bak";
        }

        //============================================================
        // Logic
        //============================================================
        public bool TryRead(bool isBackup, out string content)
        {
            content = null;
            string targetPath = isBackup ? _backupPath : _path;
            if (string.IsNullOrWhiteSpace(_path))
                return false;

            try
            {
                if (!File.Exists(targetPath))
                    return false;

                content = File.ReadAllText(targetPath, Encoding.UTF8);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
        }

        public bool TryWrite(string content, bool shouldPreserveBackup)
        {
            if (string.IsNullOrWhiteSpace(_path) || content == null)
                return false;

            string tmpPath = _path + ".tmp";
            try
            {
                string directory = Path.GetDirectoryName(Path.GetFullPath(_path));
                Directory.CreateDirectory(directory);
                byte[] bytes = new UTF8Encoding(false).GetBytes(content);
                using (FileStream stream = new FileStream(tmpPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush(true);
                }

                if (File.Exists(_path))
                    File.Replace(tmpPath, _path, shouldPreserveBackup ? null : _backupPath);
                else
                    File.Move(tmpPath, _path);

                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (NotSupportedException)
            {
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(tmpPath))
                        File.Delete(tmpPath);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}
