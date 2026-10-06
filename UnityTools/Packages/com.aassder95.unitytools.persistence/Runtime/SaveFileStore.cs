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
            return TryRead(isBackup, out content, out _);
        }

        public bool TryRead(bool isBackup, out string content, out ESaveFailure failure)
        {
            failure = ESaveFailure.None;
            content = null;
            string targetPath = isBackup ? _backupPath : _path;
            if (string.IsNullOrWhiteSpace(_path))
            {
                failure = ESaveFailure.InvalidPath;
                return false;
            }

            try
            {
                content = File.ReadAllText(targetPath, Encoding.UTF8);
                return true;
            }
            catch (FileNotFoundException)
            {
                failure = ESaveFailure.FileNotFound;
                return false;
            }
            catch (DirectoryNotFoundException)
            {
                failure = ESaveFailure.FileNotFound;
                return false;
            }
            catch (IOException)
            {
                failure = ESaveFailure.IoError;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                failure = ESaveFailure.AccessDenied;
                return false;
            }
            catch (ArgumentException)
            {
                failure = ESaveFailure.InvalidPath;
                return false;
            }
            catch (NotSupportedException)
            {
                failure = ESaveFailure.UnsupportedOperation;
                return false;
            }
        }

        public bool TryWrite(string content, bool shouldPreserveBackup)
        {
            return TryWrite(content, shouldPreserveBackup, out _);
        }

        public bool TryWrite(string content, bool shouldPreserveBackup, out ESaveFailure failure)
        {
            failure = ESaveFailure.None;
            if (string.IsNullOrWhiteSpace(_path) || content == null)
            {
                failure = string.IsNullOrWhiteSpace(_path) ? ESaveFailure.InvalidPath : ESaveFailure.InvalidArgument;
                return false;
            }

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
                failure = ESaveFailure.IoError;
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                failure = ESaveFailure.AccessDenied;
                return false;
            }
            catch (ArgumentException)
            {
                failure = ESaveFailure.InvalidPath;
                return false;
            }
            catch (NotSupportedException)
            {
                failure = ESaveFailure.UnsupportedOperation;
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
