// -----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace FaceTrackingShane
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using System.IO;
using System.Timers;
    using System.Diagnostics;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int ScreenShotInterval = 20;
        private const bool UseDecreasingInterval = false;
        private const int MinimumScreenShotInterval = 5;

        private const int ScreenShotAnimationDurationMs = 800;
        private const int FlashDurationMs = 60;

        private static readonly int Bgr32BytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;
        private readonly KinectSensorChooser sensorChooser = new KinectSensorChooser();
        private WriteableBitmap colorImageWritableBitmap;
        private byte[] colorImageData;
        private ColorImageFormat currentColorImageFormat = ColorImageFormat.Undefined;
        private string myPicturesLocation;

        private readonly Timer _screenShotAnimationTimer = new Timer()
        {
            Enabled = false,
            AutoReset = true,
            Interval = 1000 / 60 // 60 fps
        };
        private DateTime _screenShotAnimationStartTime;

        private readonly Timer _screenShotTimer = new System.Timers.Timer()
        {
            Enabled = false,
            AutoReset = true,
            Interval=1000
        };
        private int _elapsedSeconds;
        private int _screenshotIntervalSeconds;

        private string MyPicturesLocation
        {
            get
            {
                if (this.myPicturesLocation == null) {
                    this.myPicturesLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                }

                return this.myPicturesLocation;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            var faceTrackingViewerBinding = new Binding("Kinect") { Source = sensorChooser };
            faceTrackingViewer.SetBinding(FaceTrackingViewer.KinectProperty, faceTrackingViewerBinding);

            _screenShotTimer.Elapsed += new System.Timers.ElapsedEventHandler(
                (elaspedEventSender, a) =>
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var countDown = _screenshotIntervalSeconds - _elapsedSeconds;
                        lblTime.Content = countDown;
                        lblTimeShadow.Content = countDown;

                        if (_elapsedSeconds == _screenshotIntervalSeconds)
                        {
                            TakeScreenshot();
                            _elapsedSeconds = 0;

                            if (MainWindow.UseDecreasingInterval && this._screenshotIntervalSeconds > MainWindow.MinimumScreenShotInterval)
                            {
                                _screenshotIntervalSeconds /= 2;
                            }
                        }
                        else
                        {
                            _elapsedSeconds++;
                        }
                    }));
                });

            faceTrackingViewer.SkeletonsTracked += new EventHandler<Microsoft.Kinect.Toolkit.FaceTracking.FaceInFocusEventHandler>((s, args) =>
                {
                    if (!_screenShotTimer.Enabled)
                    {
                        _screenshotIntervalSeconds = MainWindow.ScreenShotInterval;
                        _elapsedSeconds = 0;
                        _screenShotTimer.Start();
                    }
                });

            faceTrackingViewer.SkeletonsLost += new EventHandler((s, a) =>
                {
                    _screenShotTimer.Stop();
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        lblTime.Content = string.Empty;
                    }));
                });

            sensorChooser.KinectChanged += SensorChooserOnKinectChanged;

            sensorChooser.Start();
        }

        private void TakeScreenshot()
        {
            DateTime now = DateTime.Now;

            BitmapSource bitmap = this.TakeScreenshotOfElement(this.MyPicturesLocation + "\\" + now.Ticks.ToString() + ".shane.png", this.MainGrid);
            this.TakeScreenshotOfElement(this.MyPicturesLocation + "\\" + now.Ticks.ToString() + ".shane.raw.png", this.ColorImage);

            this.ShowPictureTakingAnimation(bitmap);        
        }

        private void ScreenShotTimeElapsed(object elaspedEventSender, ElapsedEventArgs a)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                int elapsedMilliseconds = (int)(DateTime.Now - this._screenShotAnimationStartTime).TotalMilliseconds;
                double width = this.MainGrid.Width - (this.MainGrid.Width * (double)elapsedMilliseconds / MainWindow.ScreenShotAnimationDurationMs);
                double height = this.MainGrid.Height - (this.MainGrid.Height * (double)elapsedMilliseconds / MainWindow.ScreenShotAnimationDurationMs);

                this.ScreenshotImage.Width = width > 0 ? width : 0;
                this.ScreenshotImage.Height = height > 0 ? height : 0;

                if (elapsedMilliseconds > MainWindow.FlashDurationMs)
                {
                    this.WhiteRectangle.Visibility = System.Windows.Visibility.Hidden;
                }

                if (elapsedMilliseconds > MainWindow.ScreenShotAnimationDurationMs)
                {
                    this._screenShotAnimationTimer.Stop();
                    this.ScreenshotImage.Visibility = System.Windows.Visibility.Hidden;
                }
            }));
        }

        private void ShowPictureTakingAnimation(BitmapSource bitmap)
        {
            this._screenShotAnimationStartTime = DateTime.Now;
            this.ScreenshotImage.Visibility = System.Windows.Visibility.Visible;
            this.ScreenshotImage.Source = bitmap;

            this.WhiteRectangle.Visibility = System.Windows.Visibility.Visible;
            this._screenShotAnimationTimer.Elapsed += this.ScreenShotTimeElapsed;
            this._screenShotAnimationTimer.Start();
        }
 
         private BitmapSource TakeScreenshotOfElement(string fileName, FrameworkElement element)
         {
            RenderTargetBitmap bitmap =
                new RenderTargetBitmap((int)element.ActualWidth,
                                    (int)element.ActualHeight,
                                        96d, 96d,
                                        PixelFormats.Default);
            bitmap.Render(element);
  
            // add the RenderTargetBitmap to a Bitmapencoder
            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
  
            // save file to disk
            FileStream fs = File.Open(fileName, FileMode.OpenOrCreate);
            encoder.Save(fs);
            return bitmap;
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs kinectChangedEventArgs)
        {
            KinectSensor oldSensor = kinectChangedEventArgs.OldSensor;
            KinectSensor newSensor = kinectChangedEventArgs.NewSensor;

            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= KinectSensorOnAllFramesReady;
                oldSensor.ColorStream.Disable();
                oldSensor.DepthStream.Disable();
                oldSensor.DepthStream.Range = DepthRange.Default;
                oldSensor.SkeletonStream.Disable();
                oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                oldSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }

            if (newSensor != null)
            {
                try
                {
                    newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                    newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
                    try
                    {
                        // This will throw on non Kinect For Windows devices.
                        newSensor.DepthStream.Range = DepthRange.Near;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = true;
                    }
                    catch (InvalidOperationException)
                    {
                        newSensor.DepthStream.Range = DepthRange.Default;
                        newSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    }

                    newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    newSensor.SkeletonStream.Enable();
                    newSensor.AllFramesReady += KinectSensorOnAllFramesReady;
                }
                catch (InvalidOperationException)
                {
                    // This exception can be thrown when we are trying to
                    // enable streams on a device that has gone away.  This
                    // can occur, say, in app shutdown scenarios when the sensor
                    // goes away between the time it changed status and the
                    // time we get the sensor changed notification.
                    //
                    // Behavior here is to just eat the exception and assume
                    // another notification will come along if a sensor
                    // comes back.
                }
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            sensorChooser.Stop();
            faceTrackingViewer.Dispose();
        }

        private void KinectSensorOnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            using (var colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                {
                    return;
                }

                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat != colorImageFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat = colorImageFrame.Format;
                    this.colorImageData = new byte[colorImageFrame.PixelDataLength];
                    this.colorImageWritableBitmap = new WriteableBitmap(
                        colorImageFrame.Width, colorImageFrame.Height, 96, 96, PixelFormats.Bgr32, null);
                    ColorImage.Source = this.colorImageWritableBitmap;
                }

                colorImageFrame.CopyPixelDataTo(this.colorImageData);
                this.colorImageWritableBitmap.WritePixels(
                    new Int32Rect(0, 0, colorImageFrame.Width, colorImageFrame.Height),
                    this.colorImageData,
                    colorImageFrame.Width * Bgr32BytesPerPixel,
                    0);
            }
        }
    }
}
