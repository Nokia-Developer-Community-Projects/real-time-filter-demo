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

namespace StaticFilterViewer
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            ProcessImage();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private async void ProcessImage()
        {
            try
            {
                // Initialize a WriteableBitmap with the dimensions of the image control in XAML
                WriteableBitmap writeableBitmap = new WriteableBitmap((int)FilterEffectImage.Width, (int)FilterEffectImage.Height);

                // Example: Accessing an image stream within a standard photo chooser task callback
                // http://msdn.microsoft.com/en-us/library/windowsphone/develop/hh394019(v=vs.105).aspx
                //using (var imageStream = new StreamImageSource(e.ChosenPhoto))

                // Example: Accessing an image stream from a sample picture loaded with the project in a folder called "Pictures"
                string pictureName = "";
                //pictureName = "sample_photo_01.jpg";
                pictureName = "sample_photo_08.jpg";
                var resource = App.GetResourceStream(new Uri(string.Concat("Pictures/", pictureName), UriKind.Relative));
                using (var imageStream = new StreamImageSource(resource.Stream))
                {
                    // Applying an inbuilt filter and a custom effect to the image stream
                    //IImageProvider imageEffect = new FilterEffect(imageStream) { Filters = new List<IFilter>() { new WarpFilter(WarpEffect.Twister, 0.50) } };
                    //using (var customEffect = new PsychedelicEffect(imageEffect, 50))

                    // Applying the custom filter effect to the image stream
                    //using (var customEffect = new MirrorEffect(imageStream, MirrorEffect.MirrorType.Vertical))
                    using (var customEffect = new NegativeEffect(imageStream))
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
                System.Diagnostics.Debug.WriteLine(string.Concat("Error in ProcessImage() >>> ", ex));
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
    }
}