using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;



namespace KinectDepthRender
{
    public class SensorManager
    {
        private KinectSensor kinect = null;
        private Skeleton[] skeletonData;
        private byte[] colorPixelData;
        public short[] DepthPixelData;
        //private FaceTracker faceTracker;
        public KinectActor Player1;
        public KinectActor Player2;

        public DepthImageFormat DepthImageFormat
        {
            get
            {
                return kinect.DepthStream.Format;
            }
        }


        public Skeleton[] SkeletonData
        {
            get
            {
                return skeletonData;
            }
        }
        public bool IsSkeletonTracked { get; set; }


        public SensorManager(KinectActor p1, KinectActor p2)
        {
            Player1 = p1;
            Player2 = p2;
        }


        public void StartKinect()
        {

            //throw an exception if there isn't a kinect hooked up
            if (!KinectSensor.KinectSensors.Any())
            {
                throw new ApplicationException("no kinect sensor detected");
            }

            //grab the first connected kinect device
            kinect = KinectSensor.KinectSensors.FirstOrDefault(s => s.Status == KinectStatus.Connected);

            //senable the colorstream
            kinect.ColorStream.Enable();

            //set up our depthstream
            kinect.DepthStream.Range = DepthRange.Default;
            kinect.DepthStream.Enable(DepthImageFormat.Resolution80x60Fps30);

            //set up our skeleton sensor
            kinect.SkeletonStream.EnableTrackingInNearRange = true;
            kinect.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            kinect.SkeletonStream.Enable(
                new TransformSmoothParameters()
                {
                    Correction = 0.5f,
                    JitterRadius = 0.05f,
                    MaxDeviationRadius = 0.05f,
                    Prediction = 0.5f,
                    Smoothing = 0.5f
                });

            //initializae all of the data that stores our kinect info
            //skeletonData = new Skeleton[kinect.SkeletonStream.FrameSkeletonArrayLength]; // Allocate ST data
            skeletonData = new Skeleton[6];
            colorPixelData = new byte[kinect.ColorStream.FramePixelDataLength];
            DepthPixelData = new short[kinect.DepthStream.FramePixelDataLength];

            kinect.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(kinect_AllFramesReady); // Get Ready for Skeleton Ready Events

            kinect.Start(); // Start Kinect sensor

            //try
            //{
            //    faceTracker = new FaceTracker(kinect);
            //}
            //catch
            //{
            //    faceTracker = null;
            //}
        }

        private void kinect_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                    return;
                colorImageFrame.CopyPixelDataTo(colorPixelData);
            }

            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame == null)
                    return;
                depthImageFrame.CopyPixelDataTo(DepthPixelData);
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                    return;
                skeletonFrame.CopySkeletonDataTo(skeletonData);
            }

            foreach (Skeleton s in skeletonData)
            {
                if (s != null)
                {
                    //FaceTrackFrame faceFrame;
                    //try
                    //{
                    //    faceFrame = faceTracker.Track(kinect.ColorStream.Format, colorPixelData, kinect.DepthStream.Format, depthPixelData, s);
                    //}
                    //catch
                    //{
                    //    faceFrame = null;
                    //}
                    //AssignFaceAndSkeletonToActor(s, faceFrame);
                    AssignFaceAndSkeletonToActor(s);
                }
            }

        }

        private void AssignFaceAndSkeletonToActor(Skeleton skeleton/*, FaceTrackFrame faceFrame*/)
        {
            if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
            {
                if (Player1.ActorSkeleton.TrackingId == skeleton.TrackingId)
                {
                    Player1.ActorSkeleton = skeleton;
                    //Player1.ActorFace = faceFrame;
                    Player1.Active = true;
                }
                else if (Player2.ActorSkeleton.TrackingId == skeleton.TrackingId)
                {
                    Player2.ActorSkeleton = skeleton;
                    //Player2.ActorFace = faceFrame;
                    Player2.Active = true;
                }
                else if (Player1.Active == false)
                {
                    Player1.ActorSkeleton = skeleton;
                    //Player1.ActorFace = faceFrame;
                    Player1.Active = true;
                }
                else if (Player2.Active == false)
                {
                    Player2.ActorSkeleton = skeleton;
                    //Player2.ActorFace = faceFrame;
                    Player2.Active = true;
                }
            }
            //else
            //{
            //    if (Player1.ActorSkeleton.TrackingId == skeleton.TrackingId)
            //    {
            //        Player1.Active = false;
            //    }
                
            //    if (Player2.ActorSkeleton.TrackingId == skeleton.TrackingId)
            //    {
            //        Player2.Active = false;
            //    }
            //}
            
        }

        public int MaxElevation { get { return kinect.MaxElevationAngle; } }
        public int MinElevation { get { return kinect.MinElevationAngle; } }
        public int CurrentElevation 
        { 
            get 
            { 
                return kinect.ElevationAngle; 
            }
            set
            {
                if (value < kinect.MinElevationAngle || value > kinect.MaxElevationAngle)
                {
                    throw new Exception("Impossible elevation angle");
                }
                kinect.ElevationAngle = value;
            }
        }



        //private void DrawSkeletons()
        //{
        //    foreach (Skeleton skeleton in this.skeletonData)
        //    {
        //        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
        //        {
        //            DrawTrackedSkeletonJoints(skeleton.Joints);
        //        }
        //        else if (skeleton.TrackingState == SkeletonTrackingState.PositionOnly)
        //        {
        //            DrawSkeletonPosition(skeleton.Position);
        //        }
        //    }
        //}

        //private void DrawTrackedSkeletonJoints(JointCollection jointCollection)
        //{
        //    // Render Head and Shoulders
        //    DrawBone(jointCollection[JointType.Head], jointCollection[JointType.ShoulderCenter]);
        //    DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderLeft]);
        //    DrawBone(jointCollection[JointType.ShoulderCenter], jointCollection[JointType.ShoulderRight]);

        //    // Render Left Arm
        //    DrawBone(jointCollection[JointType.ShoulderLeft], jointCollection[JointType.ElbowLeft]);
        //    DrawBone(jointCollection[JointType.ElbowLeft], jointCollection[JointType.WristLeft]);
        //    DrawBone(jointCollection[JointType.WristLeft], jointCollection[JointType.HandLeft]);

        //    // Render Right Arm
        //    DrawBone(jointCollection[JointType.ShoulderRight], jointCollection[JointType.ElbowRight]);
        //    DrawBone(jointCollection[JointType.ElbowRight], jointCollection[JointType.WristRight]);
        //    DrawBone(jointCollection[JointType.WristRight], jointCollection[JointType.HandRight]);

        //    // Render other bones...
        //}

        //private void DrawBone(Joint jointFrom, Joint jointTo)
        //{
        //    if (jointFrom.TrackingState == JointTrackingState.NotTracked ||
        //    jointTo.TrackingState == JointTrackingState.NotTracked)
        //    {
        //        return; // nothing to draw, one of the joints is not tracked
        //    }

        //    if (jointFrom.TrackingState == JointTrackingState.Inferred ||
        //    jointTo.TrackingState == JointTrackingState.Inferred)
        //    {
        //        DrawNonTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw thin lines if either one of the joints is inferred
        //    }

        //    if (jointFrom.TrackingState == JointTrackingState.Tracked &&
        //    jointTo.TrackingState == JointTrackingState.Tracked)
        //    {
        //        DrawTrackedBoneLine(jointFrom.Position, jointTo.Position);  // Draw bold lines if the joints are both tracked
        //    }
        //}
     
        //public void DrawTrackedBoneLine(SkeletonPoint jointFrom, SkeletonPoint jointTo)
        //{
        //}


        //public void DrawNonTrackedBoneLine(SkeletonPoint jointFrom, SkeletonPoint jointTo)
        //{
        //}


        //public void DrawSkeletonPosition(SkeletonPoint position)
        //{
        //}
    }



    
}
