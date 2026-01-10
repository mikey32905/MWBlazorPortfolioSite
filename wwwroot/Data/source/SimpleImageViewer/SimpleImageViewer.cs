using SimpleImageViewer.Utilities;
using System.Windows.Media.Imaging;

namespace SimpleImageViewer.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        #region Members

         private bool _isSlideShowEnabled = false;
         private BitmapImage? _selectedImageSource;
        //private Uri _imageResourceUri;
        private string? _selectedImageFilePath;
        private bool _isGifImageVisible = false;
        private bool _isNormalImageVisible = true;



        #endregion Members

        public MainViewModel()
        {
            
        }

        #region Initializing Routines


        #endregion Initializing Routines


        #region Properties


        public bool IsSlideShowEnabled
        {
            get { return _isSlideShowEnabled; }
            set 
            {
                _isSlideShowEnabled = value;
                OnPropertyChanged("IsSlideShowEnabled");
            }
        }


        //public Uri ImageResourceUri
        //{
        //    get { return _imageResourceUri; }
        //    set { _imageResourceUri = value; }
        //}


        public BitmapImage? SelectedImageSource
        {
            get { return _selectedImageSource; }
            set 
            { 
                _selectedImageSource = value;
                OnPropertyChanged("SelectedImageSource");
            }
        }

        public string? SelectedImageFilePath
        {
            get { return _selectedImageFilePath; }
            set
            {
                _selectedImageFilePath = value;
                OnPropertyChanged("SelectedImageFilePath");
            }
        }


        public bool IsGifImageVisible
        {
            get { return _isGifImageVisible; }
            set 
            { 
                _isGifImageVisible = value;
                OnPropertyChanged("IsGifImageVisible");
            }
        }

 
        public bool IsNormalImageVisible
        {
            get { return _isNormalImageVisible; }
            set
            {
                _isNormalImageVisible = value;
                OnPropertyChanged("IsNormalImageVisible");
            }
        }


        #endregion Properties


        #region Methods

        public void UpdateImageSource(string imagePath)
        {
            SelectedImageFilePath = imagePath;
            SelectedImageSource = new BitmapImage(new Uri(imagePath, UriKind.Absolute)); 
        }


        #endregion Methods


    }
}
