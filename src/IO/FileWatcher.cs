namespace WhMgr.IO
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// File system watcher wrapper class
    /// </summary>
    public class FileWatcher
    {
        private readonly FileSystemWatcher _fsw;
        private CancellationTokenSource _changeCancellationSource;

        /// <summary>
        /// Gets the file path to watch
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Fired upon file change
        /// </summary>
        public event EventHandler<FileSystemEventArgs> Changed;

        /// <summary>
        /// Instantiate a new <see cref="FileWatcher"/> class
        /// </summary>
        /// <param name="filePath">File path to watch for changes</param>
        public FileWatcher(string filePath)
        {
            FilePath = filePath;

            _fsw = new FileSystemWatcher
            {
                Path = Directory.Exists(FilePath)
                    ? FilePath
                    : Path.GetDirectoryName(FilePath),
                // Reload on last modified time or size change
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                // Filter change based on file path or file name
                Filter = File.Exists(FilePath) ? Path.GetFileName(FilePath) : null
            };
            _fsw.Changed += OnFileSystemChanged;
        }

        /// <summary>
        /// Instantiate a new <see cref="FileWatcher"/> class
        /// </summary>
        /// <param name="filePaths">File paths to watch for changes</param>
        public FileWatcher(List<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                //FilePath = filePath;

                _fsw = new FileSystemWatcher
                {
                    Path = Directory.Exists(FilePath)
                        ? FilePath
                        : Path.GetDirectoryName(FilePath),
                    // Reload on last modified time or size change
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size,
                    // Filter change based on file path or file name
                    Filter = File.Exists(FilePath) ? Path.GetFileName(FilePath) : null
                };
                _fsw.Changed += OnFileSystemChanged;
            }
        }

        private async void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            _changeCancellationSource?.Cancel();
            _changeCancellationSource = new CancellationTokenSource();

            try
            {
                // Wait one full second in case any other changes come through in that time. If they do,
                // Task.Delay will throw an exception due to the CancellationToken being canceled.
                await Task.Delay(1000, _changeCancellationSource.Token);

                Changed?.Invoke(this, e);
                _changeCancellationSource = null;
            }
            catch (OperationCanceledException)
            {
                // Ignore cancellation exceptions thrown during notifications
            }
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
}