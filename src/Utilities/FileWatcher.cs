namespace WhMgr.Utilities
{
    using System;
    using System.IO;

    public class FileWatcher
    {
        private readonly FileSystemWatcher _fsw;

        public string FilePath { get; }

        public event EventHandler<FileChangedEventArgs> FileChanged;

        private void OnFileChanged(string filePath)
        {
            FileChanged?.Invoke(this, new FileChangedEventArgs(filePath));
        }

        public FileWatcher(string filePath)
        {
            FilePath = filePath;

            _fsw = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(FilePath),
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                Filter = Path.GetFileName(FilePath)
            };
            _fsw.Changed += (sender, e) => OnFileChanged(e.FullPath);
        }

        public void Start()
        {
            _fsw.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            _fsw.EnableRaisingEvents = false;
        }
    }

    public class FileChangedEventArgs
    {
        public string FilePath { get; set; }

        public FileChangedEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}