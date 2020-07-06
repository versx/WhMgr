namespace WhMgr.Utilities
{
    using System;
    using System.IO;

    /// <summary>
    /// File system watcher wrapper class
    /// </summary>
    public class FileWatcher
    {
        private readonly FileSystemWatcher _fsw;

        /// <summary>
        /// Gets the file path to watch
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Trigged upon file change
        /// </summary>
        public event EventHandler<FileChangedEventArgs> FileChanged;

        private void OnFileChanged(string filePath)
        {
            FileChanged?.Invoke(this, new FileChangedEventArgs(filePath));
        }

        /// <summary>
        /// Instantiate a new <see cref="FileWatcher"/> class
        /// </summary>
        /// <param name="filePath">File path to watch for changes</param>
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

        /// <summary>
        /// Start listening for file changes
        /// </summary>
        public void Start()
        {
            _fsw.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Stop listening for file changes
        /// </summary>
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