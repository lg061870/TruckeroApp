using System;
using System.IO;
using System.Threading;

namespace Truckero.Diagnostics
{
    /// <summary>
    /// Manages SQLite database clones for testing purposes.
    /// Creates a new clone only every 3 minutes to improve test performance.
    /// </summary>
    public class DatabaseCloneManager
    {
        private static readonly object _lock = new object();
        private static DateTime _lastCloneTime = DateTime.MinValue;
        private static string _currentClonePath;
        
        private readonly string _templatePath;
        private readonly string _cloneDirectory;
        private readonly TimeSpan _cloneInterval = TimeSpan.FromMinutes(3);
        
        public DatabaseCloneManager(string templatePath, string cloneDirectory = null)
        {
            _templatePath = templatePath ?? throw new ArgumentNullException(nameof(templatePath));
            _cloneDirectory = cloneDirectory ?? Path.Combine(
                Path.GetDirectoryName(_templatePath),
                "CloneDBs"
            );
            
            Directory.CreateDirectory(_cloneDirectory);
        }
        
        /// <summary>
        /// Gets the path to the current clone, creating a new one if needed based on the time interval.
        /// </summary>
        public string GetCurrentClonePath()
        {
            lock (_lock)
            {
                var now = DateTime.Now;
                
                if (_currentClonePath == null || (now - _lastCloneTime) > _cloneInterval)
                {
                    // Create a new clone if we don't have one or it's time for a refresh
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string cloneFileName = $"{Path.GetFileNameWithoutExtension(_templatePath)}_{timestamp}{Path.GetExtension(_templatePath)}";
                    _currentClonePath = Path.Combine(_cloneDirectory, cloneFileName);
                    
                    // Copy the template database to the new clone path
                    File.Copy(_templatePath, _currentClonePath, overwrite: true);
                    
                    // Clean up old clones, keeping the most recent one
                    CleanupOldClones();
                    
                    _lastCloneTime = now;
                }
                
                return _currentClonePath;
            }
        }
        
        /// <summary>
        /// Cleans up old database clones, keeping only recent ones.
        /// </summary>
        private void CleanupOldClones()
        {
            var directory = new DirectoryInfo(_cloneDirectory);
            var clonePrefix = Path.GetFileNameWithoutExtension(_templatePath);
            
            foreach (var file in directory.GetFiles())
            {
                // Skip the current clone
                if (file.FullName == _currentClonePath)
                    continue;
                    
                // Skip files that don't match our clone pattern
                if (!file.Name.StartsWith(clonePrefix))
                    continue;
                    
                try
                {
                    file.Delete();
                }
                catch (IOException)
                {
                    // File might be in use by another process, just continue
                }
            }
        }
    }
}