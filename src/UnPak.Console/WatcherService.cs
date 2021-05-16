using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DebounceThrottle;

namespace UnPak.Console
{
    public class WatcherService
    {
        private readonly string _path;
        private readonly string _targetPath;
        private FileSystemWatcher _watcher;
        private DebounceDispatcher _debounceDispatcher;
        
        public event EventHandler<WaitForChangedResult> Changed;


        public WatcherService(string path, string targetPath, int interval = 5000) {
            _path = path;
            _targetPath = targetPath;
            _debounceDispatcher = new DebounceDispatcher(interval);
        }

        private void OnChange(object sender, FileSystemEventArgs args) {
            _debounceDispatcher.Debounce(() =>
            {
                Changed?.Invoke(this, new WaitForChangedResult {ChangeType = args.ChangeType, Name = args.Name});
            });
        }

        public async Task Start(CancellationToken ctsToken) {
            try {
                ctsToken.Register(() => _watcher.Dispose());

                _watcher = new FileSystemWatcher(_path, "*.u") {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.Attributes
                                   | NotifyFilters.CreationTime
                                   | NotifyFilters.DirectoryName
                                   | NotifyFilters.FileName
                                   | NotifyFilters.LastAccess
                                   | NotifyFilters.LastWrite
                                   | NotifyFilters.Security
                                   | NotifyFilters.Size
                };
                //_watcher.Changed += (sender, args) => TriggerPack(args.ChangeType, args.FullPath);
                //_watcher.Created += (sender, args) => TriggerPack(args.ChangeType, args.FullPath);
                _watcher.Changed += OnChange;
                _watcher.Created += OnChange;
                _watcher.Deleted += OnChange;
                _watcher.Renamed += OnChange;

                _watcher.EnableRaisingEvents = true;

                await Task.Delay(Timeout.Infinite, ctsToken);
            }
            catch (TaskCanceledException) {
                //stopped by request
            }
        }
    }
}