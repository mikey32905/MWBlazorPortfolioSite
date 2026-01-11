using CloneHunter.ViewModel;
using CloneHunter.Views;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace CloneHunter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        MainViewModel ViewModel;

        SpinnerDialogView scanningView;

        #endregion Members

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;

            ViewModel.SelectedFilesDeletedCompleted += ViewModel_SelectedFilesDeletedCompleted;
            ViewModel.ScanningFilesCompleted += ViewModel_ScanningFilesCompleted; 
        }


        private void buttonBrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();

            if (dialog.ShowDialog() == true)
            {
                ViewModel.SelectedFolderPath = dialog.FolderName;
            }
        }

        private async void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedPath = ViewModel.SelectedFolderPath;

            if (string.IsNullOrEmpty(selectedPath) || !Directory.Exists(selectedPath))
            {
                MessageBox.Show("Please select a valid directory.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ViewModel.ResetDuplicateGroups();
            OpenSpinnerWindow();

            await Task.Run(() => ViewModel.PerformDirectoryScanForDuplicates());
            // ScanButton.IsEnabled = false;

        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.PerformDeleteSelectedFiles();
        }

        #region Methods

        private void ViewModel_SelectedFilesDeletedCompleted(object? sender, EventArgs e)
        {
            ScanButton.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
        }

        private void ViewModel_ScanningFilesCompleted(object? sender, EventArgs e)
        {
            scanningView.Dispatcher.Invoke(() =>
            {
                scanningView.Close();
            });
        }


        private void OpenSpinnerWindow()
        {
            scanningView = new SpinnerDialogView();
            scanningView.Owner = this;
            scanningView.Show();
        }


        #endregion Methods


    }
}