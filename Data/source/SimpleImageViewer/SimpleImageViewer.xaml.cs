using Microsoft.Win32;
using SimpleImageViewer.ViewModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SimpleImageViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        MainViewModel viewModel;

        string[] imageTypes = [".jpg", ".jpeg", ".png", ".webp", ".bmp", ".gif"];
        string[]? imageFiles;
        int currentImage = 0;

        System.Windows.Threading.DispatcherTimer slideShowTimer; 

        #endregion Members

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new MainViewModel();
            DataContext = viewModel;

           string[] args = Environment.GetCommandLineArgs();

            slideShowTimer = new System.Windows.Threading.DispatcherTimer();
            slideShowTimer.Interval = TimeSpan.FromSeconds(5);
            slideShowTimer.Tick += new EventHandler(slideShowTimer_Tick);

            if (args.Length > 1)
            {
                LoadFromStart(args);
            }
        }

        private void slideShowTimer_Tick(object? sender, EventArgs e)
        {
            MoveNextImage();
        }

        private void LoadFromStart(string[] args)
        {
            DirectoryInfo dir = new(args[1]);
           
            LoadImages(dir);

            if (dir is null)
            {
                viewModel.IsSlideShowEnabled = false;
            }
            else
            {
               viewModel.IsSlideShowEnabled = true;
            }
        }

        private void LoadImages(DirectoryInfo dir)
        {
            imageFiles = (from file in dir.GetFiles()
                          where imageTypes.Contains(file.Extension.ToLower())
                          select file.FullName).ToArray();

            currentImage = imageFiles.Length;


            MoveNextImage();
        }

        private void OpenSelectedImage(string aImagePath)
        {
            string? selectedImagePath = aImagePath;

            if (imageFiles is not null && imageFiles.Length > 0)
            {
                for (int i = 0; i < imageFiles.Length; i++)
                {
                    if (imageFiles[i] == selectedImagePath)
                    {
                        currentImage = i;
                        break;
                    }
                }

                viewModel.SelectedImageFilePath = selectedImagePath;
                viewModel.UpdateImageSource(imageFiles[currentImage]);
            }
        }

        private void MoveNextImage() 
        {
            if (imageFiles is not null && imageFiles.Length > 0)
            {
                if (currentImage >= imageFiles.Length - 1)
                {
                    currentImage = 0;
                }
                else
                {
                       currentImage++;
                }

                string fileExt = Path.GetExtension(imageFiles[currentImage]).ToLower();

                if (fileExt == ".gif")
                {
                   viewModel.IsGifImageVisible = true;
                    viewModel.IsNormalImageVisible = false;

                    viewModel.SelectedImageFilePath = imageFiles[currentImage];
                }
                else
                {
                    viewModel.IsGifImageVisible = false;
                    viewModel.IsNormalImageVisible = true;

                    viewModel.UpdateImageSource(imageFiles[currentImage]);
                }

            }
        }

        private void MovePreviousImage()
        {
            if (imageFiles is not null && imageFiles.Length > 0)
            {
                if (currentImage < 1)
                {
                    currentImage = imageFiles.Length - 1;
                }
                else
                {
                    currentImage--;
                }

                string fileExt = Path.GetExtension(imageFiles[currentImage]).ToLower();

                if (fileExt == ".gif")
                {
                    viewModel.IsGifImageVisible = true;
                    viewModel.IsNormalImageVisible = false;

                    viewModel.SelectedImageFilePath = imageFiles[currentImage]; 
                }
                else
                {
                    viewModel.IsGifImageVisible = false;
                    viewModel.IsNormalImageVisible = true;

                    viewModel.UpdateImageSource(imageFiles[currentImage]);
                }
            }
        }

        private void MenuItem_LoadDirectory_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new();

            dlg.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp;*.gif";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string? folderPath = Path.GetDirectoryName(dlg.FileName);

                if (folderPath is null)
                {
                    viewModel.IsSlideShowEnabled = false;
                    return;
                }

                DirectoryInfo dir = new(folderPath);

                LoadImages(dir);

                OpenSelectedImage(dlg.FileName);

                viewModel.IsSlideShowEnabled = true;
            }
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    MovePreviousImage();
                    break;
                case Key.Right:
                    MoveNextImage();
                    break;
                case Key.Escape:
                    if (WindowState == WindowState.Maximized)
                    {
                        WindowState = WindowState.Normal;
                    }
                    else
                    {
                        Close();
                    }
                    break;
                default:
                    break;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
             if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void MenuItem_SlideShow_Click(object sender, RoutedEventArgs e)
        {
            slideShowTimer.Start();
        }

    }
}