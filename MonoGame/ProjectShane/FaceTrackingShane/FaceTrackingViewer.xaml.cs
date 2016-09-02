// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FaceTrackingViewer.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace FaceTrackingShane
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit.FaceTracking;
    using System.Linq;

    using Point = System.Windows.Point;
    using System.Windows.Media.Imaging;
    using FaceTrackingShane;

    /// <summary>
    /// Class that uses the Face Tracking SDK to display a face mask for
    /// tracked skeletons
    /// </summary>
    public partial class FaceTrackingViewer : UserControl, IDisposable
    {
        public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
            "Kinect", 
            typeof(KinectSensor), 
            typeof(FaceTrackingViewer), 
            new PropertyMetadata(
                null, (o, args) => ((FaceTrackingViewer)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

        private const uint MaxMissedFrames = 100;

        private readonly Dictionary<int, SkeletonFaceTracker> trackedSkeletons = new Dictionary<int, SkeletonFaceTracker>();

        private byte[] colorImage;

        private ColorImageFormat colorImageFormat = ColorImageFormat.Undefined;

        private short[] depthImage;

        private DepthImageFormat depthImageFormat = DepthImageFormat.Undefined;

        private bool disposed;

        private Skeleton[] skeletonData;

        public FaceTrackingViewer()
        {
            this.InitializeComponent();
        }

        ~FaceTrackingViewer()
        {
            this.Dispose(false);
        }

        public KinectSensor Kinect
        {
            get
            {
                return (KinectSensor)this.GetValue(KinectProperty);
            }

            set
            {
                this.SetValue(KinectProperty, value);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                this.ResetFaceTracking();

                this.disposed = true;
            }
        }

        AccessoryImageSet[] Accessories = null;
        private void LoadAccessoryImages()
        {
            Accessories = AccessoryImageSet.LoadDefinitionFile();
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            if (Accessories == null)
            {
                this.LoadAccessoryImages();
            }


            base.OnRender(drawingContext);

            int i = 0;
            foreach (SkeletonFaceTracker faceInformation in this.trackedSkeletons.Values)
            {
                if (i == 0)
                {
                    faceInformation.DrawFaceModel(drawingContext);
                    faceInformation.DrawAccessory(drawingContext, this.Kinect);
                }
                else if (i == 1)
                {
                    faceInformation.DrawFaceModel(drawingContext);
                    faceInformation.DrawAccessory(drawingContext, this.Kinect);
                }

                i++;
            }
        }

        private void OnAllFramesReady(object sender, AllFramesReadyEventArgs allFramesReadyEventArgs)
        {
            ColorImageFrame colorImageFrame = null;
            DepthImageFrame depthImageFrame = null;
            SkeletonFrame skeletonFrame = null;

            try
            {
                colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame();
                depthImageFrame = allFramesReadyEventArgs.OpenDepthImageFrame();
                skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame();

                if (colorImageFrame == null || depthImageFrame == null || skeletonFrame == null)
                {
                    return;
                }

                // Check for image format changes.  The FaceTracker doesn't
                // deal with that so we need to reset.
                if (this.depthImageFormat != depthImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.depthImage = null;
                    this.depthImageFormat = depthImageFrame.Format;
                }

                if (this.colorImageFormat != colorImageFrame.Format)
                {
                    this.ResetFaceTracking();
                    this.colorImage = null;
                    this.colorImageFormat = colorImageFrame.Format;
                }

                // Create any buffers to store copies of the data we work with
                if (this.depthImage == null)
                {
                    this.depthImage = new short[depthImageFrame.PixelDataLength];
                }

                if (this.colorImage == null)
                {
                    this.colorImage = new byte[colorImageFrame.PixelDataLength];
                }
                
                // Get the skeleton information
                if (this.skeletonData == null || this.skeletonData.Length != skeletonFrame.SkeletonArrayLength)
                {
                    this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                }

                colorImageFrame.CopyPixelDataTo(this.colorImage);
                depthImageFrame.CopyPixelDataTo(this.depthImage);
                skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                // Update the list of trackers and the trackers with the current frame information
                foreach (Skeleton skeleton in this.skeletonData)
                {
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked
                        || skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
                    {
                        // We want keep a record of any skeleton, tracked or untracked.
                        if (!this.trackedSkeletons.ContainsKey(skeleton.TrackingId))
                        {
                            SkeletonFaceTracker tracker = new SkeletonFaceTracker(imageContainer, Kinect);
                            tracker.AccessoryImageSet = ChooseAccessory();
                            tracker.Skeleton = skeleton;
                            this.trackedSkeletons.Add(skeleton.TrackingId, tracker);
                        }

                        // Give each tracker the upated frame.
                        SkeletonFaceTracker skeletonFaceTracker;
                        if (this.trackedSkeletons.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker))
                        {
                            skeletonFaceTracker.OnFrameReady(this.Kinect, colorImageFormat, colorImage, depthImageFormat, depthImage, skeleton);
                            skeletonFaceTracker.LastTrackedFrame = skeletonFrame.FrameNumber;
                        }
                    }
                }

                this.RemoveOldTrackers(skeletonFrame.FrameNumber);

                if (trackedSkeletons.Count > 0)
                {
                    if (this.SkeletonsTracked != null)
                    {
                        this.SkeletonsTracked.Invoke(this, new FaceInFocusEventHandler(this.skeletonData));
                    }
                }
                else
                {
                    if (this.SkeletonsLost != null)
                    {
                        this.SkeletonsLost(this, new EventArgs());
                    }
                }


                this.InvalidateVisual();
            }
            finally
            {
                if (colorImageFrame != null)
                {
                    colorImageFrame.Dispose();
                }

                if (depthImageFrame != null)
                {
                    depthImageFrame.Dispose();
                }

                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }
        }

        /// <summary>
        /// TODO - anPalm - chooses which accessory to use, if any - TEST THIS
        /// </summary>
        public AccessoryImageSet ChooseAccessory()
        {
            Random rand = new Random();
            if (rand.Next(10) == 1 && Accessories.Length > 0) //just assigning an accessory to 10% of the skeletons
            {
                return Accessories[rand.Next(Accessories.Length)];
            }
            else return null;
        }

        public event EventHandler<FaceInFocusEventHandler> SkeletonsTracked;
        public event EventHandler SkeletonsLost;

        private void OnSensorChanged(KinectSensor oldSensor, KinectSensor newSensor)
        {
            if (oldSensor != null)
            {
                oldSensor.AllFramesReady -= this.OnAllFramesReady;
                this.ResetFaceTracking();
            }

            if (newSensor != null)
            {
                newSensor.AllFramesReady += this.OnAllFramesReady;
            }
        }

        /// <summary>
        /// Clear out any trackers for skeletons we haven't heard from for a while
        /// </summary>
        private void RemoveOldTrackers(int currentFrameNumber)
        {
            var trackersToRemove = new List<int>();

            foreach (var tracker in this.trackedSkeletons)
            {
                uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value.LastTrackedFrame;
                if (missedFrames > MaxMissedFrames)
                {
                    // There have been too many frames since we last saw this skeleton
                    trackersToRemove.Add(tracker.Key);
                }
            }

            foreach (int trackingId in trackersToRemove)
            {
                this.RemoveTracker(trackingId);
            }
        }

        private void RemoveTracker(int trackingId)
        {
            this.trackedSkeletons[trackingId].Dispose();
            this.trackedSkeletons.Remove(trackingId);
        }

        private void ResetFaceTracking()
        {
            foreach (int trackingId in new List<int>(this.trackedSkeletons.Keys))
            {
                this.RemoveTracker(trackingId);
            }
        }

        public class SkeletonFaceTracker : IDisposable
        {
            private const bool CycleFaces = false;
            private const int FaceMissingThreshold = 50;

            //private EnumIndexableCollection<FeaturePoint, PointF> facePoints;
            private Microsoft.Kinect.Toolkit.FaceTracking.Rect faceRect;
            public Skeleton Skeleton = new Skeleton();

            private FaceTracker faceTracker;

            private bool lastFaceTrackSucceeded;

            private SkeletonTrackingState skeletonTrackingState;

            private static readonly FaceImageData[] _faces = new FaceImageData[] {
                new FaceImageData("Images/BlondShane.png", 165, 380, 235, 230),
                new FaceImageData("Images/MooseShane.png", 50, 118, 60, 60),
                new FaceImageData("Images/ConstipatedShane.png", 105, 185, 105, 105),
                new FaceImageData("Images/ShaneHeadband.png", 65, 175, 120, 95),
                new FaceImageData("Images/stoned-shane.png", 110, 235, 150, 130),
                new FaceImageData("Images/HatShane.png", 60, 140, 70, 70),
                new FaceImageData("Images/JesusShane.png", 80, 135, 80, 100),
                new FaceImageData("Images/head.png", 75, 205, 145, 110),
                new FaceImageData("Images/shane-santa.png", 20, 61, 44, 38),
                new FaceImageData("Images/head2.png", 65, 160, 127, 93),
                new FaceImageData("Images/shane-afro.png", 88, 178, 84, 84),
                new FaceImageData("Images/chief-shane.png", 99, 243, 81, 84),
                new FaceImageData("Images/sexy-santa-shane.png", 33, 124, 86, 72),
            };

            private int countFaceMissing = 0;
            private DateTime _lastCycle = DateTime.MinValue;
            private int _imageIndex = 0;

            private FaceImageData ImageData
            {
                get
                {
                    if (SkeletonFaceTracker.CycleFaces && DateTime.Now.Subtract(_lastCycle) > TimeSpan.FromSeconds(5))
                    {
                        _imageIndex++;
                        _imageIndex %= _faces.Length;
                        _lastCycle = DateTime.Now;
                        _shaneHeadImage.Source = new BitmapImage(new Uri(@"/FaceTrackingShane;component/" + _faces[_imageIndex].fileName, UriKind.Relative));
                    }

                    return _faces[_imageIndex];
                }
            }

            private readonly Image _shaneHeadImage;

            private readonly Canvas _headImageCanvas;

            public int LastTrackedFrame { get; set; }

            public AccessoryImageSet AccessoryImageSet = null;

            public KinectSensor Kinect;


            public SkeletonFaceTracker(Canvas headImageCanvas, KinectSensor sensor)
            {
                Kinect = sensor;
                _headImageCanvas = headImageCanvas;
                _shaneHeadImage = new Image();
                _headImageCanvas.Children.Add(_shaneHeadImage);
                _shaneHeadImage.Source = new BitmapImage(new Uri(@"/FaceTrackingShane;component/" + _faces[_imageIndex].fileName, UriKind.Relative));
                _shaneHeadImage.Visibility = Visibility.Hidden;
                this.countFaceMissing = FaceMissingThreshold;

                if (!SkeletonFaceTracker.CycleFaces)
                {
                    // randomize which face we get
                    this._imageIndex = new Random().Next(SkeletonFaceTracker._faces.Length);
                    _shaneHeadImage.Source = new BitmapImage(new Uri(@"/FaceTrackingShane;component/" + SkeletonFaceTracker._faces[_imageIndex].fileName, UriKind.Relative));
                }
            }

            public void Dispose()
            {
                using (this.faceTracker) { }
                this.faceTracker = null;
                _headImageCanvas.Children.Remove(_shaneHeadImage);
            }

            public bool HasActiveFace
            {
                get
                {
                    return this.countFaceMissing < SkeletonFaceTracker.FaceMissingThreshold;
                }
            }


            public void DrawAccessory(DrawingContext dc, KinectSensor sensor)
            {
                if (this.AccessoryImageSet == null)
                {
                    return;
                }
                this.AccessoryImageSet.DrawModel(this.Skeleton, dc, sensor);
            }


            public void DrawFaceModel(DrawingContext dc)
            {
                if (!this.lastFaceTrackSucceeded || this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    ColorImagePoint point = Kinect.MapSkeletonPointToColor(Skeleton.Joints[JointType.Head].Position, Kinect.ColorStream.Format);
                    
                    // place shane head at right location based on head joint without scaling etc.. since we don't have face data
                    Canvas.SetLeft(this._shaneHeadImage, point.X - this._shaneHeadImage.Width / 2);
                    Canvas.SetTop(this._shaneHeadImage, point.Y - this._shaneHeadImage.Height / 2);

                    return;
                }
                else if (!HasActiveFace)
                {
                    _shaneHeadImage.Visibility = Visibility.Visible;
                }

                float pictureToFaceScaleX = (float)_shaneHeadImage.Source.Width / (float)this.ImageData.width;
                float pictureToFaceScaleY = (float)_shaneHeadImage.Source.Height / (float)this.ImageData.height;

                var fullImageWidth = this.faceRect.Width * pictureToFaceScaleX;
                var fullImageHeight = this.faceRect.Height * pictureToFaceScaleY;

                int scaledXOffset = (int)((float)this.ImageData.left / (float)_shaneHeadImage.Source.Width * fullImageWidth);
                int scaledYOffset = (int)((float)this.ImageData.top / (float)_shaneHeadImage.Source.Height * fullImageHeight);

                Canvas.SetTop(_shaneHeadImage, this.faceRect.Top - scaledYOffset);
                Canvas.SetLeft(_shaneHeadImage, this.faceRect.Left - scaledXOffset);
                _shaneHeadImage.Width = fullImageWidth;
                _shaneHeadImage.Height = fullImageHeight;
            }

            /// <summary>
            /// Updates the face tracking information for this skeleton
            /// </summary>
            internal void OnFrameReady(KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest)
            {
                this.skeletonTrackingState = skeletonOfInterest.TrackingState;

                if (this.skeletonTrackingState != SkeletonTrackingState.Tracked)
                {
                    // nothing to do with an untracked skeleton.
                    return;
                }

                if (this.faceTracker == null)
                {
                    try
                    {
                        this.faceTracker = new FaceTracker(kinectSensor);
                    }
                    catch (InvalidOperationException)
                    {
                        // During some shutdown scenarios the FaceTracker
                        // is unable to be instantiated.  Catch that exception
                        // and don't track a face.
                        Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
                        this.faceTracker = null;
                    }
                }

                if (this.faceTracker != null)
                {
                    FaceTrackFrame frame = this.faceTracker.Track(
                        colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);

                    this.lastFaceTrackSucceeded = frame.TrackSuccessful;
                    if (this.lastFaceTrackSucceeded)
                    {
                        //if (faceTriangles == null)
                        //{
                        //    // only need to get this once.  It doesn't change.
                        //    faceTriangles = frame.GetTriangles();
                        //}

                        //this.facePoints = frame.GetProjected3DShape();

                        this.faceRect = frame.FaceRect;
                    }
                }
            }
        }
    }
}