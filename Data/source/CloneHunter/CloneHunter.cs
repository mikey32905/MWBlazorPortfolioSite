using CloneHunter.Models;
using CloneHunter.Utilities;
using System.Collections.ObjectModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace CloneHunter.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        #region Members

        private ObservableCollection<DuplicateGroup> _duplicateGroups = new ObservableCollection<DuplicateGroup>();

        private string _selectedFolderPath = "";

        private string _statusLabelText = "Ready";
        private bool _isScanButtonEnabled = true;
        private bool _isSpinnerVisible = false;

        #region Event Members

        public event EventHandler SelectedFilesDeletedCompleted;
        public event EventHandler ScanningFilesCompleted;

        #endregion Event Member

        #endregion Members


        public MainViewModel()
        {
            //DuplicateGroups = new ObservableCollection<DuplicateGroup>();
        }

        #region Properties

        public ObservableCollection<DuplicateGroup> DuplicateGroups 
        {
            get { return _duplicateGroups; }
            set
            {
                _duplicateGroups = value;
                OnPropertyChanged(nameof(DuplicateGroups));
            }
        }
        

        public string StatusLabelText
        {
            get { return _statusLabelText; }
            set 
            { 
                _statusLabelText = value;
                OnPropertyChanged(nameof(StatusLabelText));
            }
        }

        public string SelectedFolderPath
        {
            get { return _selectedFolderPath; }
            set
            { 
                _selectedFolderPath = value;
                OnPropertyChanged(nameof(SelectedFolderPath));
            }
        }

        public bool IsScanButtonEnabled
        {
            get { return _isScanButtonEnabled; }
            set
            {
                _isScanButtonEnabled = value;
                OnPropertyChanged(nameof(IsScanButtonEnabled));
            }
        }


        public bool IsSpinnerVisible
        {
            get { return _isSpinnerVisible; }
            set 
            { 
                _isSpinnerVisible = value;
                OnPropertyChanged(nameof(IsSpinnerVisible));
            }
        }



        #endregion Properties


        #region Methods

        public void ResetDuplicateGroups()
        {
            DuplicateGroups.Clear();
        }

        public void PerformDirectoryScanForDuplicates()
        {
            IsScanButtonEnabled = false;
            IsSpinnerVisible = true;
            StatusLabelText = "Scanning...";

            try
            {
                var duplicates = FindDuplicates(SelectedFolderPath);

                DuplicateGroups = new ObservableCollection<DuplicateGroup>(duplicates);

                StatusLabelText = $"Found {DuplicateGroups.Count} duplicate group(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during scan: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusLabelText = "Error";
            }
            finally
            {
                IsScanButtonEnabled = true;
                ScanningFilesCompleted?.Invoke(this, EventArgs.Empty);
                //IsSpinnerVisible = false;
            }
        }

        private List<DuplicateGroup> FindDuplicates(string path)
        {
            var fileHashes = new Dictionary<string, List<string>>();
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                try
                {
                    var hash = ComputeFileHash(file);
                    if (!fileHashes.ContainsKey(hash))
                        fileHashes[hash] = new List<string>();
                    fileHashes[hash].Add(file);
                }
                catch { }
            }

            var duplicateGroups = new List<DuplicateGroup>();
            int groupNum = 1;

            foreach (var kvp in fileHashes.Where(x => x.Value.Count > 1))
            {
                var group = new DuplicateGroup
                {
                    Header = $"Duplicate Group {groupNum++} ({kvp.Value.Count} files, {GetFileSize(kvp.Value[0])})"
                };

                foreach (var filePath in kvp.Value)
                {
                    group.Files.Add(new FileItem { FullPath = filePath });
                }

                duplicateGroups.Add(group);
            }

            return duplicateGroups;
        }

        private string ComputeFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        private string GetFileSize(string filePath)
        {
            var size = new FileInfo(filePath).Length;
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }


        public void PerformDeleteSelectedFiles()
        {
            var selectedFiles = DuplicateGroups
                    .SelectMany(g => g.Files)
                    .Where(f => f.IsSelected)
                    .ToList();

            if (selectedFiles.Count == 0)
            {
                MessageBox.Show("No files selected for deletion.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to delete {selectedFiles.Count} file(s)?",
                "Confirm Deletion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                int deletedCount = 0;
                foreach (var file in selectedFiles)
                {
                    try
                    {
                        File.Delete(file.FullPath);
                        deletedCount++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete {file.FullPath}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                MessageBox.Show($"Successfully deleted {deletedCount} file(s).", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                // Scan_Click(sender, e);
                SelectedFilesDeletedCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion Methods

    }
}
