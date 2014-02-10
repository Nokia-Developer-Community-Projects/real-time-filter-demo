/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation. 
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners. 
 * See LICENSE.TXT for license information.
 */

using NISDKExtendedEffects.ImageEffects;
using Nokia.Graphics.Imaging;
using RealtimeFilterDemo.Resources;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
//using System.Windows.Media;
using Windows.UI;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;

namespace RealtimeFilterDemo
{
    public class NokiaImagingSDKEffects : ICameraEffect
    {
        private PhotoCaptureDevice _photoCaptureDevice = null;
        private CameraPreviewImageSource _cameraPreviewImageSource = null;
        private FilterEffect _filterEffect = null;
        private CustomEffectBase _customEffect = null;
        private int _effectIndex = 0;
        private Semaphore _semaphore = new Semaphore(1, 1);

        public String EffectName { get; private set; }

        public PhotoCaptureDevice PhotoCaptureDevice
        {
            set
            {
                if (_photoCaptureDevice != value)
                {
                    while (!_semaphore.WaitOne(100));

                    _photoCaptureDevice = value;

                    Initialize();

                    _semaphore.Release();
                }
            }
        }

        ~NokiaImagingSDKEffects()
        {
            while (!_semaphore.WaitOne(100));

            Uninitialize();

            _semaphore.Release();
        }

        public async Task GetNewFrameAndApplyEffect(IBuffer frameBuffer, Size frameSize)
        {
            if (_semaphore.WaitOne(500))
            {
                var scanlineByteSize = (uint)frameSize.Width * 4; // 4 bytes per pixel in BGRA888 mode
                var bitmap = new Bitmap(frameSize, ColorMode.Bgra8888, scanlineByteSize, frameBuffer);

                if (_filterEffect != null)
                {
                    var renderer = new BitmapRenderer(_filterEffect, bitmap);
                    await renderer.RenderAsync();
                }
                else if (_customEffect != null)
                {
                    var renderer = new BitmapRenderer(_customEffect, bitmap);
                    await renderer.RenderAsync();
                }
                else
                {
                    var renderer = new BitmapRenderer(_cameraPreviewImageSource, bitmap);
                    await renderer.RenderAsync();
                }

                _semaphore.Release();
            }
        }

        public void NextEffect()
        {
            if (_semaphore.WaitOne(500))
            {
                Uninitialize();

                _effectIndex++;

                if (_effectIndex >= _effectCount)
                {
                    _effectIndex = 0;
                }

                Initialize();

                _semaphore.Release();
            }
        }

        public void PreviousEffect()
        {
            if (_semaphore.WaitOne(500))
            {
                Uninitialize();
                
                _effectIndex--;

                if (_effectIndex < 0)
                {
                    _effectIndex = _effectCount - 1;
                }

                Initialize();

                _semaphore.Release();
            }
        }

        private void Uninitialize()
        {
            if (_cameraPreviewImageSource != null)
            {
                _cameraPreviewImageSource.Dispose();
                _cameraPreviewImageSource = null;
            }

            if (_filterEffect != null)
            {
                _filterEffect.Dispose();
                _filterEffect = null;
            }

            if (_customEffect != null)
            {
                _customEffect.Dispose();
                _customEffect = null;
            }
        }

        private void Initialize()
        {
            var filters = new List<IFilter>();
            var nameFormat = "{0}/" + _effectCount + " - {1}";

            App.AssignedColorCache = new Dictionary<uint, Color>(); // Reset
            _cameraPreviewImageSource = new CameraPreviewImageSource(_photoCaptureDevice);

            switch (_effectIndex)
            {
                case 0:
                    {
                        EffectName = String.Format(nameFormat, 1, AppResources.Filter_Lomo);
                        filters.Add(new LomoFilter(0.5, 0.5, LomoVignetting.High, LomoStyle.Yellow));
                    }
                    break;

                case 1:
                    {
                        EffectName = String.Format(nameFormat, 2, AppResources.Filter_MagicPen);
                        filters.Add(new MagicPenFilter());
                    }
                    break;

                case 2:
                    {
                        EffectName = String.Format(nameFormat, 3, AppResources.Filter_Grayscale);
                        filters.Add(new GrayscaleFilter());
                    }
                    break;

                case 3:
                    {
                        EffectName = String.Format(nameFormat, 4, AppResources.Filter_Antique);
                        filters.Add(new AntiqueFilter());
                    }
                    break;

                case 4:
                    {
                        EffectName = String.Format(nameFormat, 5, AppResources.Filter_Stamp);
                        filters.Add(new StampFilter(4, 0.3));
                    }
                    break;

                case 5:
                    {
                        EffectName = String.Format(nameFormat, 6, AppResources.Filter_Cartoon);
                        filters.Add(new CartoonFilter(false));
                    }
                    break;

                case 6:
                    {
                        EffectName = String.Format(nameFormat, 7, AppResources.Filter_Sepia);
                        filters.Add(new SepiaFilter());
                    }
                    break;

                case 7:
                    {
                        EffectName = String.Format(nameFormat, 8, AppResources.Filter_Sharpness);
                        filters.Add(new SharpnessFilter(7));
                    }
                    break;

                case 8:
                    {
                        EffectName = String.Format(nameFormat, 9, AppResources.Filter_AutoEnhance);
                        filters.Add(new AutoEnhanceFilter());
                    }
                    break;

                case 9:
                    {
                        EffectName = String.Format(nameFormat, 10, AppResources.Filter_None);
                        // Runs at 18 FPS with NoEffect and 18-19 FPS with nothing at all, so NoEffect() costs about 0.5 FPS
                        _customEffect = new NoEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 10:
                    {
                        EffectName = String.Format(nameFormat, 11, AppResources.Filter_Custom);
                        _customEffect = new CustomEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 11:
                    {
                        EffectName = String.Format(nameFormat, 12, "Built-in BrightnessFilter >>> +0.50");
                        filters.Add(new BrightnessFilter(0.50));
                    }
                    break;

                case 12:
                    {
                        EffectName = String.Format(nameFormat, 13, "Custom BrightnessEffect >>> +0.50");
                        _customEffect = new BrightnessEffect(_cameraPreviewImageSource, 0.50);
                    }
                    break;

                case 13:
                    {
                        EffectName = String.Format(nameFormat, 14, "Built-in BrightnessFilter >>> -0.50");
                        filters.Add(new BrightnessFilter(-0.50));
                    }
                    break;

                case 14:
                    {
                        EffectName = String.Format(nameFormat, 15, "Custom BrightnessEffect >>> -0.50");
                        _customEffect = new BrightnessEffect(_cameraPreviewImageSource, -0.50);
                    }
                    break;

                case 15:
                    {
                        EffectName = String.Format(nameFormat, 16, "Built-in ColorAdjustFilter >>> Red at -1.0");
                        filters.Add(new ColorAdjustFilter(-1.0, 0, 0));
                    }
                    break;

                case 16:
                    {
                        EffectName = String.Format(nameFormat, 17, "Custom ColorAdjustEffect >>> Red at -1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, -1.0, 0, 0);
                    }
                    break;

                case 17:
                    {
                        EffectName = String.Format(nameFormat, 18, "Built-in ColorAdjustFilter >>> Red at +1.0");
                        filters.Add(new ColorAdjustFilter(1.0, 0, 0));
                    }
                    break;

                case 18:
                    {
                        EffectName = String.Format(nameFormat, 19, "Custom ColorAdjustEffect >>> Red at +1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 1.0, 0, 0);
                    }
                    break;

                case 19:
                    {
                        EffectName = String.Format(nameFormat, 20, "Built-in ColorAdjustFilter >>> Green at -1.0");
                        filters.Add(new ColorAdjustFilter(0, -1.0, 0));
                    }
                    break;

                case 20:
                    {
                        EffectName = String.Format(nameFormat, 21, "Custom ColorAdjustEffect >>> Green at -1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, -1.0, 0);
                    }
                    break;

                case 21:
                    {
                        EffectName = String.Format(nameFormat, 22, "Built-in ColorAdjustFilter >>> Green at +1.0");
                        filters.Add(new ColorAdjustFilter(0, 1.0, 0));
                    }
                    break;

                case 22:
                    {
                        EffectName = String.Format(nameFormat, 23, "Custom ColorAdjustEffect >>> Green at +1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, 1.0, 0);
                    }
                    break;

                case 23:
                    {
                        EffectName = String.Format(nameFormat, 24, "Built-in ColorAdjustFilter >>> Blue at -1.0");
                        filters.Add(new ColorAdjustFilter(0, 0, -1.0));
                    }
                    break;

                case 24:
                    {
                        EffectName = String.Format(nameFormat, 25, "Custom ColorAdjustEffect >>> Blue at -1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, 0, -1.0);
                    }
                    break;

                case 25:
                    {
                        EffectName = String.Format(nameFormat, 26, "Built-in ColorAdjustFilter >>> Blue at +1.0");
                        filters.Add(new ColorAdjustFilter(0, 0, 1.0));
                    }
                    break;

                case 26:
                    {
                        EffectName = String.Format(nameFormat, 27, "Custom ColorAdjustEffect >>> Blue at +1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, 0, 1.0);
                    }
                    break;

                case 27:
                    {
                        EffectName = String.Format(nameFormat, 28, "Built-in MirrorFilter");
                        filters.Add(new MirrorFilter());
                    }
                    break;

                case 28:
                    {
                        EffectName = String.Format(nameFormat, 29, "Custom MirrorEffect >>> Horizontal");
                        _customEffect = new MirrorEffect(_cameraPreviewImageSource, MirrorEffect.MirrorType.Horizontal);
                    }
                    break;

                case 29:
                    {
                        EffectName = String.Format(nameFormat, 30, "Built-in MirrorFilter and RotateFilter");
                        filters.Add(new RotationFilter(270)); 
                        filters.Add(new MirrorFilter());
                        filters.Add(new RotationFilter(90));
                    }
                    break;

                case 30:
                    {
                        EffectName = String.Format(nameFormat, 31, "Custom MirrorEffect >>> Vertical");
                        _customEffect = new MirrorEffect(_cameraPreviewImageSource, MirrorEffect.MirrorType.Vertical);
                    }
                    break;

                case 31:
                    {
                        EffectName = String.Format(nameFormat, 32, "Built-in GrayscaleFilter");
                        filters.Add(new GrayscaleFilter());
                    }
                    break;

                case 32:
                    {
                        EffectName = String.Format(nameFormat, 33, "Custom GrayscaleEffect");
                        _customEffect = new GrayscaleEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 33:
                    {
                        EffectName = String.Format(nameFormat, 34, "Built-in GrayscaleNegativeFilter");
                        filters.Add(new GrayscaleNegativeFilter());
                    }
                    break;

                case 34:
                    {
                        EffectName = String.Format(nameFormat, 35, "Custom GrayscaleNegativeEffect");
                        _customEffect = new GrayscaleNegativeEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 35:
                    {
                        EffectName = String.Format(nameFormat, 36, "Built-in NegativeFilter");
                        filters.Add(new NegativeFilter());
                    }
                    break;

                case 36:
                    {
                        EffectName = String.Format(nameFormat, 37, "Custom NegativeEffect");
                        _customEffect = new NegativeEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 37:
                    {
                        EffectName = String.Format(nameFormat, 38, "PixelationEffect - Scale: 5");
                        _customEffect = new PixelationEffect(_cameraPreviewImageSource, 5);
                    }
                    break;

                case 38:
                    {
                        EffectName = String.Format(nameFormat, 39, "PixelationEffect - Scale: 15");
                        _customEffect = new PixelationEffect(_cameraPreviewImageSource, 15);
                    }
                    break;

                case 39:
                    {
                        EffectName = String.Format(nameFormat, 40, "PixelationEffect - Scale: 35");
                        _customEffect = new PixelationEffect(_cameraPreviewImageSource, 35);
                    }
                    break;

                case 40:
                    {
                        EffectName = String.Format(nameFormat, 42, "PsychedelicEffect - Factor: 25 with WarpEffect.Twister - 0.50");
                        IImageProvider imageEffect = new FilterEffect(_cameraPreviewImageSource)
                        {
                            Filters = new List<IFilter>() { new WarpFilter(WarpEffect.Twister, 0.50) }
                        };
                        _customEffect = new PsychedelicEffect(imageEffect, 25);
                    }
                    break;

                case 41:
                    {
                        EffectName = String.Format(nameFormat, 42, "PsychedelicEffect - Factor: 50 with WarpEffect.Twister - 0.50");
                        IImageProvider imageEffect = new FilterEffect(_cameraPreviewImageSource)
                        {
                            Filters = new List<IFilter>() { new WarpFilter(WarpEffect.Twister, 0.50) }
                        };
                        _customEffect = new PsychedelicEffect(imageEffect, 50);

                    }
                    break;

                case 42:
                    {
                        EffectName = String.Format(nameFormat, 43, "SkipPixelEffect - RowSkip: 3 | ColumnSkip: 3");
                        _customEffect = new SkipPixelEffect(_cameraPreviewImageSource, 3, 3);
                    }
                    break;

                case 43:
                    {
                        EffectName = String.Format(nameFormat, 44, "SkipPixelEffect - RowSkip: 8 | ColumnSkip: 8");
                        _customEffect = new SkipPixelEffect(_cameraPreviewImageSource, 8, 8);
                    }
                    break;

                case 44:
                    {
                        EffectName = String.Format(nameFormat, 45, "SkipPixelEffect - RowSkip: 13 | ColumnSkip: 13");
                        _customEffect = new SkipPixelEffect(_cameraPreviewImageSource, 13, 13);
                    }
                    break;

                case 45:
                    {

                        EffectName = String.Format(nameFormat, 46, "QuantizeColorEffect without Cache - 16 color");
                        Dictionary<uint, Color> assignedColorCache = null;
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref assignedColorCache);
                    }
                    break;

                case 46:
                    {
                        EffectName = String.Format(nameFormat, 47, "QuantizeColorEffect with Cache - 16 color");
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref App.AssignedColorCache);
                    }
                    break;

                case 47:
                    {
                        List<Color> targetColors = new List<Color>();
                        targetColors.Add(Color.FromArgb(255, 0, 0, 0)); // Black
                        targetColors.Add(Color.FromArgb(255, 0, 0, 128)); // Low Blue (Navy)
                        targetColors.Add(Color.FromArgb(255, 0, 128, 0)); // Low Green (Green)
                        //targetColors.Add(Color.FromArgb(255, 0, 128, 128)); // Low Cyan (Teal)
                        targetColors.Add(Color.FromArgb(255, 128, 0, 0)); // Low Red (Maroon)
                        //targetColors.Add(Color.FromArgb(255, 128, 0, 128)); // Low Magenta (Purple)
                        //targetColors.Add(Color.FromArgb(255, 128, 128, 0)); // Brown (Olive)
                        //targetColors.Add(Color.FromArgb(255, 192, 192, 192)); // Light Gray (Silver)
                        //targetColors.Add(Color.FromArgb(255, 169, 169, 169)); // Dark Gray (Gray)
                        targetColors.Add(Color.FromArgb(255, 0, 0, 255)); // High Blue (Blue)
                        targetColors.Add(Color.FromArgb(255, 0, 255, 0)); // High Green (Lime)
                        targetColors.Add(Color.FromArgb(255, 0, 255, 255)); // High Cyan (Aqua)
                        targetColors.Add(Color.FromArgb(255, 255, 0, 0)); // High Red (Red)
                        targetColors.Add(Color.FromArgb(255, 255, 0, 255)); // High Magenta (Fuchsia)
                        targetColors.Add(Color.FromArgb(255, 255, 165, 0)); // Orange
                        targetColors.Add(Color.FromArgb(255, 255, 255, 0)); // Yellow
                        targetColors.Add(Color.FromArgb(255, 255, 255, 255)); // White

                        EffectName = String.Format(nameFormat, 48, "QuantizeColorEffect with Cache - Custom Colors");
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref App.AssignedColorCache, targetColors);

                        // Experimenting - creating MagicPen effect
                        //filters.Add(new SketchFilter(SketchMode.Color));
                        //filters.Add(new SharpnessFilter(7));
                        //filters.Add(new DespeckleFilter(DespeckleLevel.High));
                        //filters.Add(new SketchFilter(SketchMode.Gray));

                        //IImageProvider imageEffect = new FilterEffect(_cameraPreviewImageSource) { Filters = new List<IFilter>() { new SketchFilter(SketchMode.Color) } };
                        //_customEffect = new QuantizeColorEffect(imageEffect, ref App.AssignedColorCache, targetColors);
                    }
                    break;

                case 48:
                    {
                        EffectName = String.Format(nameFormat, 49, "Built-in SepiaFilter");
                        filters.Add(new SepiaFilter());
                    }
                    break;

                case 49:
                    {
                        EffectName = String.Format(nameFormat, 50, "Custom SepiaEffect - 0.42 (default)");
                        _customEffect = new SepiaEffect(_cameraPreviewImageSource, 0.42);
                    }
                    break;

                case 50:
                    {
                        EffectName = String.Format(nameFormat, 51, "Custom SepiaEffect - 0.32");
                        _customEffect = new SepiaEffect(_cameraPreviewImageSource, 0.32);
                    }
                    break;

                case 51:
                    {
                        EffectName = String.Format(nameFormat, 52, "Custom SepiaEffect - 0.62");
                        _customEffect = new SepiaEffect(_cameraPreviewImageSource, 0.62);
                    }
                    break;

                case 52:
                    {
                        EffectName = String.Format(nameFormat, 53, "Built-in GrayscaleFilter");
                        filters.Add(new GrayscaleFilter());
                    }
                    break;

                case 53:
                    {
                        EffectName = String.Format(nameFormat, 54, "Custom GrayscaleEffect");
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource);
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.2126, 0.7152, 0.0722); // Defined Algo 1 - Default
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.212671, 0.71516, 0.072169); // CIE Y Algo
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.500, 0.419, 0.081); // R-Y Algo
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.299, 0.587, 0.114); // Defined Algo 2
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.241, 0.691, 0.068, true); // Defined Algo 3
                        //Experiments:
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.3333, 0.3333, 0.3333);
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.3990, 0.3870, 0.2140);
                        _customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.3126, 0.5152, 0.0722); // very close to SDK
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.2276, 0.7152, 0.0822);
                        //_customEffect = new GrayscaleEffect(_cameraPreviewImageSource, 0.2526, 0.6652, 0.0822);

                        //EffectName = String.Format(nameFormat, 52, "Built-in WarpFilter");
                        //filters.Add(new WarpFilter(WarpEffect.HappyFool, 0.5)); // 10-11 FPS
                        //filters.Add(new WarpFilter(WarpEffect.Twister, 0.5));
                        
                    }
                    break;

            }

            if (filters.Count > 0)
            {
                _filterEffect = new FilterEffect(_cameraPreviewImageSource)
                {
                    Filters = filters
                };
            }
        }

        private int _effectCount = 54;  // Remember to increment by one with each case added above.
    }
}