using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using StaticFilterViewer.Resources;
using System.Windows.Media.Imaging;
using NISDKExtendedEffects.ImageEffects;
using Nokia.Graphics.Imaging;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Phone.Tasks;


namespace StaticFilterViewer
{
    public partial class MainPage : PhoneApplicationPage
    {
        private string m_LocalPictureName = "";
        private SDKCustomEffects m_SDKCustomEffects = null;
        private bool m_Busy = false;
        private int m_CurrentLocalImageIndex = 0;
        private List<string> m_LocalImages = new List<string>();
        ApplicationBarIconButton m_PreviousButton = new ApplicationBarIconButton();
        ApplicationBarIconButton m_NextButton = new ApplicationBarIconButton();
        ApplicationBarIconButton m_NextImageButton = new ApplicationBarIconButton();
        ApplicationBarIconButton m_PreviousImageButton = new ApplicationBarIconButton();
        //ApplicationBarIconButton m_SaveButton = new ApplicationBarIconButton();
        ApplicationBarMenuItem m_SaveButton = new ApplicationBarMenuItem();
        ApplicationBarMenuItem m_ChooseImageButton = new ApplicationBarMenuItem();

        public bool Busy
        {
            get
            {
                return m_Busy;
            }
            set
            {
                if (value.Equals(true))
                {
                    m_PreviousButton.IsEnabled = false;
                    m_NextButton.IsEnabled = false;
                    m_NextImageButton.IsEnabled = false;
                    m_PreviousImageButton.IsEnabled = false;
                    m_SaveButton.IsEnabled = false;
                    m_ChooseImageButton.IsEnabled = false;
                }
                else
                {
                    m_PreviousButton.IsEnabled = true;
                    m_NextButton.IsEnabled = true;
                    m_NextImageButton.IsEnabled = true;
                    m_PreviousImageButton.IsEnabled = true;
                    m_SaveButton.IsEnabled = true;
                    m_ChooseImageButton.IsEnabled = true;
                }
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            BuildApplicationBar();
        }
        
        private void BuildApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();

            m_PreviousButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/previous.png", UriKind.Relative));
            m_PreviousButton.Text = "Previous FX";
            m_PreviousButton.Click += PreviousButton_Click;
            ApplicationBar.Buttons.Add(m_PreviousButton);

            m_NextButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/next.png", UriKind.Relative));
            m_NextButton.Text = "Next FX";
            m_NextButton.Click += NextButton_Click;
            ApplicationBar.Buttons.Add(m_NextButton);

            m_NextImageButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.arrow.up.png", UriKind.Relative));
            m_NextImageButton.Text = "Image Up";
            m_NextImageButton.Click += NextImageButton_Click;
            ApplicationBar.Buttons.Add(m_NextImageButton);

            m_PreviousImageButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/appbar.arrow.down.png", UriKind.Relative));
            m_PreviousImageButton.Text = "Image Down";
            m_PreviousImageButton.Click += PreviousImageButton_Click;
            ApplicationBar.Buttons.Add(m_PreviousImageButton);

            //m_SaveButton = new ApplicationBarIconButton(new Uri("/Assets/Icons/save.png", UriKind.Relative));
            //m_SaveButton.Text = "Save";
            //m_SaveButton.Click += SaveButton_Click;
            //ApplicationBar.Buttons.Add(m_SaveButton);

            m_SaveButton = new ApplicationBarMenuItem();
            m_SaveButton.Text = "Save Image";
            m_SaveButton.Click += SaveButton_Click;
            ApplicationBar.MenuItems.Add(m_SaveButton);

            m_ChooseImageButton = new ApplicationBarMenuItem();
            m_ChooseImageButton.Text = "Choose Image";
            m_ChooseImageButton.Click += ChooseImageButton_Click;
            ApplicationBar.MenuItems.Add(m_ChooseImageButton);

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            Initialize();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            Uninitialize();
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);
        }

        private async Task Initialize()
        {
            Busy = true;
            m_SDKCustomEffects = new SDKCustomEffects();

            await LoadLocalImageNames();
            m_CurrentLocalImageIndex = m_LocalImages.Count() - 1; // Start with the last image in the list
            m_LocalPictureName = m_LocalImages[m_CurrentLocalImageIndex];
            LoadLocalImage();
            
            // Start off at the last effect
            m_SDKCustomEffects.PreviousEffect();
            StatusTextBlock.Text = m_SDKCustomEffects.EffectName;
            await ProcessImage();
            Busy = false;
        }

        private void Uninitialize()
        {
            StatusTextBlock.Text = "";

            if (m_SDKCustomEffects != null)
            {
                m_SDKCustomEffects.Uninitialize();
                m_SDKCustomEffects = null;
            }
        }

        private async void NextButton_Click(object sender, EventArgs e)
        {
            Busy = true;
            m_SDKCustomEffects.NextEffect();
            StatusTextBlock.Text = m_SDKCustomEffects.EffectName;
            await ProcessImage();
            Busy = false;
        }

        private async void PreviousButton_Click(object sender, EventArgs e)
        {
            Busy = true;
            m_SDKCustomEffects.PreviousEffect();
            StatusTextBlock.Text = m_SDKCustomEffects.EffectName;
            await ProcessImage();
            Busy = false;
        }

        private async void NextImageButton_Click(object sender, EventArgs e)
        {
            Busy = true;

            // Get the next image, and go back to the beginning if we are already at the end
            m_CurrentLocalImageIndex++;
            if (m_CurrentLocalImageIndex >= m_LocalImages.Count())
            {
                m_CurrentLocalImageIndex = 0;
            }
            m_LocalPictureName = m_LocalImages[m_CurrentLocalImageIndex];

            LoadLocalImage();
            await ProcessImage();
            Busy = false;
        }

        private async void PreviousImageButton_Click(object sender, EventArgs e)
        {
            Busy = true;

            // Get the previous image, and go to the end if we are already at the beginning
            m_CurrentLocalImageIndex--;
            if (m_CurrentLocalImageIndex <= 0)
            {
                m_CurrentLocalImageIndex = m_LocalImages.Count() - 1;
            }
            m_LocalPictureName = m_LocalImages[m_CurrentLocalImageIndex];

            LoadLocalImage();
            await ProcessImage();
            Busy = false;
        }

        void ChooseImageButton_Click(object sender, EventArgs e)
        {
            Busy = true;
            PhotoChooserTask photoChooser = new PhotoChooserTask();
            photoChooser.Completed += ChooseImageButton_Callback;
            photoChooser.Show();
        }

        private async void ChooseImageButton_Callback(object sender, PhotoResult e)
        {
            // Example: Accessing an image stream within a standard photo chooser task callback
            // http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh394019(v=vs.105).aspx

            if (e.TaskResult != TaskResult.OK || e.ChosenPhoto == null)
                return;

            try
            {
                if (m_SDKCustomEffects == null)
                {
                    await Initialize();
                }

                e.ChosenPhoto.Position = 0;
                StreamImageSource imageStream = new StreamImageSource(e.ChosenPhoto);
                m_SDKCustomEffects.StreamImage = imageStream;
                await ProcessImage();
            }
            catch (Exception ex)
            {
                Busy = false;
                System.Diagnostics.Debug.WriteLine(string.Concat("Error in ChooseImageButton_Callback() >>> ", ex));
            }

            Busy = false;
        }

        private async void SaveButton_Click(object sender, EventArgs e)
        {
            Busy = true;

            try
            {

                if (m_SDKCustomEffects == null)
                    return;

                var jpegRenderer = new JpegRenderer(m_SDKCustomEffects.CustomEffect);

                // Jpeg renderer gives the raw buffer for the filtered image.
                IBuffer jpegOutput = await jpegRenderer.RenderAsync();

                // Save the image as a jpeg to the saved pictures album.
                // NOTE: Must include this using statement for jpegOutput to have the AsStream() method
                // using System.Runtime.InteropServices.WindowsRuntime;
                MediaLibrary library = new MediaLibrary();
                string fileName = string.Format("CustonEffectImage_{0:G}", DateTime.Now);
                var picture = library.SavePicture(fileName, jpegOutput.AsStream());

                MessageBox.Show("Image saved!");
            }
            catch (Exception ex)
            {
                Busy = false;
                System.Diagnostics.Debug.WriteLine(string.Concat("Error in SaveButton_Click() >>> ", ex));
            }

            Busy = false;
        }

        private async Task ProcessImage()
        {
            try
            {
                // Initialize a WriteableBitmap with the dimensions of the image control in XAML
                WriteableBitmap writeableBitmap = new WriteableBitmap((int)FilterEffectImage.Width, (int)FilterEffectImage.Height);

                // Rendering the resulting image to a WriteableBitmap
                using (var renderer = new WriteableBitmapRenderer(m_SDKCustomEffects.CustomEffect, writeableBitmap))
                {
                    // Applying the WriteableBitmap to our xaml image control
                    FilterEffectImage.Source = await renderer.RenderAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Concat("Error in ProcessLocalImage() >>> ", ex));
            }
        }

        private void LoadLocalImage()
        {
            try
            {
                // Example: Accessing an image stream from a sample picture loaded with the project in a folder called "Pictures"
                var resource = App.GetResourceStream(new Uri(string.Concat("Pictures/", m_LocalPictureName), UriKind.Relative));
                StreamImageSource imageStream = new StreamImageSource(resource.Stream);
                m_SDKCustomEffects.StreamImage = imageStream;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Concat("Error in LoadLocalImage() >>> ", ex));
            }
        }

        private async Task LoadLocalImageNames()
        {
            try
            {
                // Get the Pictures folder that is created dynamically in the ApplicationData by the app.
                //StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
                //var dataFolder = await local.CreateFolderAsync("Pictures", CreationCollisionOption.OpenIfExists);

                // Get the Pictures folder that is pre-loaded with the app installation files
                StorageFolder dataFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync("Pictures");

                // Loop through the Picutes folder and load up the image names into a list
                m_LocalImages = new List<string>();
                foreach (var item in await dataFolder.GetFilesAsync())
                {
                    m_LocalImages.Add(item.Name);
                }

                m_LocalImages.Sort();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Concat("Error in LoadLocalImageNames() >>> ", ex));
            }
        }

        private async void ProcessImage_v1()
        {
            try
            {
                // Initialize a WriteableBitmap with the dimensions of the image control in XAML
                WriteableBitmap writeableBitmap = new WriteableBitmap((int)FilterEffectImage.Width, (int)FilterEffectImage.Height);

                // Example: Accessing an image stream within a standard photo chooser task callback
                // http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh394019(v=vs.105).aspx
                //using (var imageStream = new StreamImageSource(e.ChosenPhoto))

                // Example: Accessing an image stream from a sample picture loaded with the project in a folder called "Pictures"
                //m_LocalPictureName = "sample_photo_01.jpg";
                m_LocalPictureName = "sample_photo_08.jpg";
                var resource = App.GetResourceStream(new Uri(string.Concat("Pictures/", m_LocalPictureName), UriKind.Relative));
                using (var imageStream = new StreamImageSource(resource.Stream))
                {
                    // Applying an inbuilt filter and a custom effect to the image stream
                    //IImageProvider imageEffect = new FilterEffect(imageStream) { Filters = new List<IFilter>() { new WarpFilter(WarpEffect.Twister, 0.50) } };
                    //using (var customEffect = new PsychedelicEffect(imageEffect, 50))

                    // Applying the custom filter effect to the image stream
                    using (var customEffect = new SepiaEffect(imageStream, 0.42))
                    //using (var customEffect = new MirrorEffect(imageStream, MirrorEffect.MirrorType.Vertical))
                    //using (var customEffect = new NegativeEffect(imageStream))
                    //using (var customEffect = new FilterEffect(imageStream) { Filters = new List<IFilter>() { new NegativeFilter() } })
                    //using (var customEffect = new GrayscaleNegativeEffect(imageStream))
                    //using (var customEffect = new FilterEffect(imageStream) { Filters = new List<IFilter>() { new GrayscaleNegativeFilter() } })
                    //using (var customEffect = new BrightnessEffect(imageStream, 0.50))
                    //using (var customEffect = new FilterEffect(imageStream) { Filters = new List<IFilter>() { new GrayscaleFilter() } })
                    //using (var customEffect = new GrayscaleEffect(imageStream))
                    //using (var customEffect = new GrayscaleEffect(imageStream, 0.2126, 0.7152, 0.0722)) // Algorithm 1 - Default
                    //using (var customEffect = new GrayscaleEffect(imageStream, 0.299, 0.587, 0.114)) // Algorithm 2
                    //using (var customEffect = new GrayscaleEffect(imageStream, 0.3333, 0.3333, 0.3333)) 
                    {
                        // Rendering the resulting image to a WriteableBitmap
                        using (var renderer = new WriteableBitmapRenderer(customEffect, writeableBitmap))
                        {
                            // Applying the WriteableBitmap to our xaml image control
                            FilterEffectImage.Source = await renderer.RenderAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Concat("Error in ProcessImage_v1() >>> ", ex));
            }
        }
    }
}