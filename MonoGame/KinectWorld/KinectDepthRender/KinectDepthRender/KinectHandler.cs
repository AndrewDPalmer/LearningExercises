using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace KinectDepthRender
{
    public class KinectManager
    {
        public List<KinectActor> Actors = new List<KinectActor>();
        private List<SensorManager> _sensors = new List<SensorManager>();
        public List<SensorManager> Sensors
        {
            get
            {
                return _sensors;
            }
        }
        private GameBase _game;
        private int _maxSuppoortedSensors = 1;

        public int SensorCount
        {
            get
            {
                return _sensors.Count;
            }
        }

        public KinectManager(GameBase game)
        {
            if (_maxSuppoortedSensors <= 0)
            {
                throw new Exception("You must support at least 1 sensor");
            }
            _game = game;
        }

        public void SetupKinectHandler()
        {
            //throw an exception if there isn't a kinect hooked up
            if (!KinectSensor.KinectSensors.Any())
            {
                throw new ApplicationException("no kinect sensor detected");
            }

            int count = 1;
            foreach (KinectSensor s in KinectSensor.KinectSensors)
            {
                if (s.Status == KinectStatus.Connected)
                {
                    KinectActor p1 = new KinectActor(_game);
                    KinectActor p2 = new KinectActor(_game);
                    _sensors.Add(new SensorManager(p1, p2));
                    Actors.Add(p1);
                    Actors.Add(p2);
                    count++;
                }
                if (count > _maxSuppoortedSensors)
                {
                    return;
                }
            }
        }

        public void StartSensors()
        {
            foreach (SensorManager s in _sensors)
            {
                s.StartKinect();
            }
        }


    }
}
