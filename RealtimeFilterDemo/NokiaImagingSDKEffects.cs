/*
 * Copyright © 2013 Nokia Corporation. All rights reserved.
 * Nokia and Nokia Connecting People are registered trademarks of Nokia Corporation.
 * Other product and company names mentioned herein may be trademarks
 * or trade names of their respective owners.
 * See LICENSE.TXT for license information.
 */

using NISDKExtendedEffects.ImageEffects;
using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;

//using System.Windows.Media;

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
        private Size _frameSize;

        public String EffectName { get; private set; }

        public PhotoCaptureDevice PhotoCaptureDevice
        {
            set
            {
                if (_photoCaptureDevice != value)
                {
                    while (!_semaphore.WaitOne(100)) ;

                    _photoCaptureDevice = value;

                    Initialize();

                    _semaphore.Release();
                }
            }
        }

        ~NokiaImagingSDKEffects()
        {
            while (!_semaphore.WaitOne(100)) ;

            Uninitialize();

            _semaphore.Release();
        }

        public async Task GetNewFrameAndApplyEffect(IBuffer frameBuffer, Size frameSize)
        {
            if (_semaphore.WaitOne(500))
            {
                _frameSize = frameSize;

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

            App.AssignedColorCache = new Dictionary<uint, uint>(); // Reset
            _cameraPreviewImageSource = new CameraPreviewImageSource(_photoCaptureDevice);

            switch (_effectIndex)
            {
                case 0:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "NoEffect");
                        // Runs at 18 FPS with NoEffect and 18-19 FPS with nothing at all, so NoEffect() costs about 0.5 FPS
                        _customEffect = new NoEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 1:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in BrightnessFilter >>> +0.50");
                        filters.Add(new BrightnessFilter(0.50));
                    }
                    break;

                case 2:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom BrightnessEffect >>> +0.50");
                        _customEffect = new BrightnessEffect(_cameraPreviewImageSource, 0.50);
                    }
                    break;

                case 3:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in BrightnessFilter >>> -0.50");
                        filters.Add(new BrightnessFilter(-0.50));
                    }
                    break;

                case 4:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom BrightnessEffect >>> -0.50");
                        _customEffect = new BrightnessEffect(_cameraPreviewImageSource, -0.50);
                    }
                    break;

                case 5:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in ColorAdjustFilter >>> Red at -1.0");
                        filters.Add(new ColorAdjustFilter(-1.0, 0, 0));
                    }
                    break;

                case 6:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom ColorAdjustEffect >>> Red at -1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, -1.0, 0, 0);
                    }
                    break;

                case 7:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in ColorAdjustFilter >>> Red at +1.0");
                        filters.Add(new ColorAdjustFilter(1.0, 0, 0));
                    }
                    break;

                case 8:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom ColorAdjustEffect >>> Red at +1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 1.0, 0, 0);
                    }
                    break;

                case 9:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in ColorAdjustFilter >>> Green at -1.0");
                        filters.Add(new ColorAdjustFilter(0, -1.0, 0));
                    }
                    break;

                case 10:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom ColorAdjustEffect >>> Green at -1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, -1.0, 0);
                    }
                    break;

                case 11:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in ColorAdjustFilter >>> Green at +1.0");
                        filters.Add(new ColorAdjustFilter(0, 1.0, 0));
                    }
                    break;

                case 12:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom ColorAdjustEffect >>> Green at +1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, 1.0, 0);
                    }
                    break;

                case 13:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in ColorAdjustFilter >>> Blue at -1.0");
                        filters.Add(new ColorAdjustFilter(0, 0, -1.0));
                    }
                    break;

                case 14:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom ColorAdjustEffect >>> Blue at -1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, 0, -1.0);
                    }
                    break;

                case 15:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in ColorAdjustFilter >>> Blue at +1.0");
                        filters.Add(new ColorAdjustFilter(0, 0, 1.0));
                    }
                    break;

                case 16:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom ColorAdjustEffect >>> Blue at +1.0");
                        _customEffect = new ColorAdjustEffect(_cameraPreviewImageSource, 0, 0, 1.0);
                    }
                    break;

                case 17:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in MirrorFilter");
                        filters.Add(new MirrorFilter());
                    }
                    break;

                case 18:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom MirrorEffect >>> Horizontal");
                        _customEffect = new MirrorEffect(_cameraPreviewImageSource, MirrorEffect.MirrorType.Horizontal);
                    }
                    break;

                case 19:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in MirrorFilter and RotateFilter");
                        filters.Add(new RotationFilter(270));
                        filters.Add(new MirrorFilter());
                        filters.Add(new RotationFilter(90));
                    }
                    break;

                case 20:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom MirrorEffect >>> Vertical");
                        _customEffect = new MirrorEffect(_cameraPreviewImageSource, MirrorEffect.MirrorType.Vertical);
                    }
                    break;

                case 21:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in GrayscaleFilter");
                        filters.Add(new GrayscaleFilter());
                    }
                    break;

                case 22:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom GrayscaleEffect");
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
                    }
                    break;

                case 23:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in GrayscaleNegativeFilter");
                        filters.Add(new GrayscaleNegativeFilter());
                    }
                    break;

                case 24:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom GrayscaleNegativeEffect");
                        _customEffect = new GrayscaleNegativeEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 25:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in NegativeFilter");
                        filters.Add(new NegativeFilter());
                    }
                    break;

                case 26:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom NegativeEffect");
                        _customEffect = new NegativeEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 27:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "PixelationEffect - Scale: 5");
                        _customEffect = new PixelationEffect(_cameraPreviewImageSource, 5);
                    }
                    break;

                case 28:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "PixelationEffect - Scale: 15");
                        _customEffect = new PixelationEffect(_cameraPreviewImageSource, 15);
                    }
                    break;

                case 29:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "PixelationEffect - Scale: 35");
                        _customEffect = new PixelationEffect(_cameraPreviewImageSource, 35);
                    }
                    break;

                case 30:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "PsychedelicEffect - Factor: 25 with WarpEffect.Twister - 0.50");
                        IImageProvider imageEffect = new FilterEffect(_cameraPreviewImageSource)
                        {
                            Filters = new List<IFilter>() { new WarpFilter(WarpEffect.Twister, 0.50) }
                        };
                        _customEffect = new PsychedelicEffect(imageEffect, 25);
                    }
                    break;

                case 31:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "PsychedelicEffect - Factor: 50 with WarpEffect.Twister - 0.50");
                        IImageProvider imageEffect = new FilterEffect(_cameraPreviewImageSource)
                        {
                            Filters = new List<IFilter>() { new WarpFilter(WarpEffect.Twister, 0.50) }
                        };
                        _customEffect = new PsychedelicEffect(imageEffect, 50);
                    }
                    break;

                case 32:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "SkipPixelEffect - RowSkip: 3 | ColumnSkip: 3");
                        _customEffect = new SkipPixelEffect(_cameraPreviewImageSource, 3, 3);
                    }
                    break;

                case 33:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "SkipPixelEffect - RowSkip: 8 | ColumnSkip: 8");
                        _customEffect = new SkipPixelEffect(_cameraPreviewImageSource, 8, 8);
                    }
                    break;

                case 34:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "SkipPixelEffect - RowSkip: 13 | ColumnSkip: 13");
                        _customEffect = new SkipPixelEffect(_cameraPreviewImageSource, 13, 13);
                    }
                    break;

                case 35:
                    {
                        //// Dismal performance without Cache
                        //EffectName = String.Format(nameFormat, (_effectIndex + 1), "QuantizeColorEffect without Cache - 16 color");
                        //Dictionary<uint, Color> assignedColorCache = null;
                        //_customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref assignedColorCache,
                        //    null, QuantizeColorEffect.ColorPalette.Color16);

                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Inbuilt CartoonFilter");
                        filters.Add(new CartoonFilter());
                    }
                    break;

                case 36:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "QuantizeColorEffect with Cache - Half of Web Safe Colors");
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref App.AssignedColorCache,
                            null, QuantizeColorEffect.ColorPalette.WebSafeHalf);
                    }
                    break;

                case 37:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "QuantizeColorEffect with Cache - Web Safe Colors");
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref App.AssignedColorCache,
                            null, QuantizeColorEffect.ColorPalette.WebSafe);
                    }
                    break;

                case 38:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "QuantizeColorEffect with Cache - X11 Colors");
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref App.AssignedColorCache,
                            null, QuantizeColorEffect.ColorPalette.X11);
                    }
                    break;

                case 39:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "QuantizeColorEffect with Cache - 16 Colors");
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref App.AssignedColorCache,
                            null, QuantizeColorEffect.ColorPalette.Color16);
                    }
                    break;

                case 40:
                    {
                        List<uint> targetColors = new List<uint>();
                        targetColors.Add(0xff000000 | (0 << 16) | (0 << 8) | 0); // Black
                        targetColors.Add(0xff000000 | (0 << 16) | (0 << 8) | 128); // Low Blue (Navy)
                        targetColors.Add(0xff000000 | (0 << 16) | (128 << 8) | 0); // Low Green (Green)
                        targetColors.Add(0xff000000 | (0 << 16) | (128 << 128) | 0); // Low Cyan (Teal)
                        targetColors.Add(0xff000000 | (128 << 16) | (0 << 8) | 0); // Low Red (Maroon)
                        targetColors.Add(0xff000000 | (175 << 16) | (238 << 8) | 238); // PaleTurquoise
                        targetColors.Add(0xff000000 | (255 << 16) | (69 << 8) | 0); // OrangeRed
                        targetColors.Add(0xff000000 | (255 << 16) | (99 << 8) | 71); // Tomato
                        targetColors.Add(0xff000000 | (255 << 16) | (0 << 8) | 255); // High Magenta (Fuchsia)
                        targetColors.Add(0xff000000 | (255 << 16) | (165 << 8) | 0); // Orange
                        targetColors.Add(0xff000000 | (255 << 16) | (255 << 8) | 0); // Yellow
                        targetColors.Add(0xff000000 | (47 << 16) | (79 << 8) | 79); // DarkSlateGray
                        targetColors.Add(0xff000000 | (255 << 16) | (255 << 8) | 255); // White
                        targetColors.Add(0xff000000 | (250 << 16) | (250 << 8) | 210); // LightGoldenrodYellow
                        targetColors.Add(0xff000000 | (176 << 16) | (196 << 8) | 222); // LightSteelBlue
                        targetColors.Add(0xff000000 | (255 << 16) | (255 << 8) | 240); // Ivory
                        targetColors.Add(0xff000000 | (255 << 16) | (245 << 8) | 238); // Seashell
                        targetColors.Add(0xff000000 | (245 << 16) | (245 << 8) | 220); // Beige
                        targetColors.Add(0xff000000 | (70 << 16) | (130 << 8) | 180); // SteelBlue
                        targetColors.Add(0xff000000 | (250 << 16) | (235 << 8) | 215); // AntiqueWhite

                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "QuantizeColorEffect with Cache - Custom Colors");
                        _customEffect = new QuantizeColorEffect(_cameraPreviewImageSource, ref App.AssignedColorCache, targetColors);
                    }
                    break;

                case 41:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Built-in SepiaFilter");
                        filters.Add(new SepiaFilter());
                    }
                    break;

                case 42:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom SepiaEffect - 0.42 (default)");
                        _customEffect = new SepiaEffect(_cameraPreviewImageSource, 0.42);
                    }
                    break;

                case 43:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom SepiaEffect - 0.32");
                        _customEffect = new SepiaEffect(_cameraPreviewImageSource, 0.32);
                    }
                    break;

                case 44:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom SepiaEffect - 0.62");
                        _customEffect = new SepiaEffect(_cameraPreviewImageSource, 0.62);
                    }
                    break;

                case 45:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom CannyEdgeDetection");
                        _customEffect = new CannyEdgeDetection(_cameraPreviewImageSource);
                    }
                    break;

                case 46:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom OtsuThresholdEffect");
                        _customEffect = new OtsuThresholdEffect(_cameraPreviewImageSource);
                    }
                    break;

                case 47:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom SobelEdgeDetection");
                        _customEffect = new SobelEdgeDetection(_cameraPreviewImageSource);
                    }
                    break;

                case 48:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom BlobCounter");

                        //CannyEdgeDetection gives better performance but sobel is much faster
                        var sobelDetection = new OtsuThresholdEffect(_cameraPreviewImageSource);

                        _customEffect = new BlobCounter(sobelDetection)
                        {
                            //Draws detected objects as rectangle, for more information http://www.aforgenet.com/articles/shape_checker/
                            HasPreview = true,
                            PreviewCount = 10,
                            ObjectsOrder = NISDKExtendedEffects.Entities.ObjectsOrder.Area
                        };
                    }

                    break;

                case 49:
                    {
                        EffectName = String.Format(nameFormat, (_effectIndex + 1), "Custom QuadTransformation");

                        NISDKExtendedEffects.Entities.EdgePoints points = new NISDKExtendedEffects.Entities.EdgePoints()
                        {
                            TopLeft = new System.Windows.Point(120, 50),
                            TopRight = new System.Windows.Point(550, 0),
                            BottomLeft = new System.Windows.Point(100, 440),
                            BottomRight = new System.Windows.Point(450, 300)
                        };

                        //NISDKExtendedEffects.Entities.EdgePoints points = new NISDKExtendedEffects.Entities.EdgePoints()
                        //{
                        //    TopLeft = new System.Windows.Point(20, 20),
                        //    TopRight = new System.Windows.Point(150, 0),
                        //    BottomLeft = new System.Windows.Point(50, 100),
                        //    BottomRight = new System.Windows.Point(125, 125)
                        //};

                        //NISDKExtendedEffects.Entities.EdgePoints points = new NISDKExtendedEffects.Entities.EdgePoints()
                        //{
                        //    TopLeft = new System.Windows.Point(50, 25),
                        //    TopRight = new System.Windows.Point(625, 35),
                        //    BottomLeft = new System.Windows.Point(25, 475),
                        //    BottomRight = new System.Windows.Point(640, 480)
                        //};

                        var estimatedSize = points.EstimatedRectangleSize();

                        var customEffect = new QuadTransformation(_cameraPreviewImageSource, estimatedSize, NISDKExtendedEffects.Entities.QuadDirection.QuadToRect, points);

                        var reframingFilter = new ReframingFilter(new Windows.Foundation.Rect(0, 0, estimatedSize.Width, estimatedSize.Height), 0);

                        _filterEffect = new FilterEffect(customEffect)
                        {
                            Filters = new IFilter[] { reframingFilter }
                        };
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

        private int _effectCount = 50;  // Remember to increment by one with each case added above.
    }
}