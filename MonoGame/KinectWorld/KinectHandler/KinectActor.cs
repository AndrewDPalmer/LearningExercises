using Microsoft.Kinect;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
using KinectHandler;


namespace KinectHandler
{

    public abstract class ModelBase
    {
    }


    public class KinectActor
    {
        public Skeleton ActorSkeleton;
        //public FaceTrackFrame ActorFace;

        //public ModelBase Model;


        public bool Active { get; set; }
        protected GameBase _parent;
        public ZoomLevel ZoomLevel { get; set; }
        public int ScreenLocationX { get; set; }
        public int ScreenLocationY { get; set; }

        public KinectActor(KinectActor other)
        {
            _parent = other._parent;
            ZoomLevel = other.ZoomLevel;
            ActorSkeleton = other.ActorSkeleton;
            ScreenLocationX = other.ScreenLocationX;
            ScreenLocationY = other.ScreenLocationY;
            Active = other.Active;
            //ActorFace = other.ActorFace;
        }


        public KinectActor(GameBase parentBooth, int defaultLocationX, int defaultLocationY) 
        {
            _parent = parentBooth;
            ZoomLevel = new ZoomLevel(0.6);
            ActorSkeleton = new Skeleton();
            ScreenLocationX = defaultLocationX;
            ScreenLocationY = defaultLocationY;
        }

        public KinectActor(GameBase parentBooth)
        {
            _parent = parentBooth;
            ZoomLevel = new ZoomLevel(0.2);
            ActorSkeleton = new Skeleton();
        }



    }
}
