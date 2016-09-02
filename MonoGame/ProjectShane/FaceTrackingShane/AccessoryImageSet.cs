using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Kinect;
using System.Xml;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Media;
using System.Windows.Input;


namespace FaceTrackingShane
{

    public class AccessoryImageSet
    {
        public string Name = "";
        public int FrequencyPercentage = 0;
        List<ImageJointCombo> ImageCombos = new List<ImageJointCombo>();

        public static AccessoryImageSet[] LoadDefinitionFile()
        {
            XmlTextReader reader = new XmlTextReader(new Uri("AccessoryImageSets.xml", UriKind.Relative).ToString());
            List<AccessoryImageSet> sets = new List<AccessoryImageSet>();
            AccessoryImageSet set = null;

            while (reader.Read())  
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "AccessoryImageSet")
                {
                    set = new AccessoryImageSet();
                    set.Name = reader.GetAttribute("ID");
                    set.FrequencyPercentage = int.Parse(reader.GetAttribute("Frequency"));
                    sets.Add(set);
                }
                else if (reader.NodeType == XmlNodeType.Element && reader.Name == "ImageJointCombo")
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(reader.GetAttribute("Image"), UriKind.Relative));
                    JointType jt = (JointType)Enum.Parse(typeof(JointType), reader.GetAttribute("JointType"));
                    Vector vec = new Vector(double.Parse(reader.GetAttribute("OffsetX")), double.Parse(reader.GetAttribute("OffsetY")));
                    ImageJointCombo combo = new ImageJointCombo(img, jt, vec, set.Name);

                    set.AddImageJointCombo(combo);
                }
            }

            AccessoryImageSet[] returnSets = new AccessoryImageSet[sets.Count];
            for (int i = 0; i < sets.Count; i++)
            {
                returnSets[i] = sets[i];
            }

            return returnSets;

        }

        public class ImageJointCombo
        {
            public Image Image;
            public JointType JointType;
            public Vector Offset;
            public string Name;
           

            public ImageJointCombo(Image img, JointType jointType, Vector offset, string name)
            {
                Image = img;
                JointType = jointType;
                Offset = offset;
                Name = name;
            }
        }

        public void AddImageJointCombo(ImageJointCombo combo)
        {
            ImageCombos.Add(combo);
        }

        public void DrawModel(Skeleton skeleton, DrawingContext dc, KinectSensor sensor)
        {
            foreach (ImageJointCombo combo in this.ImageCombos)
            {
                Vector coord = GetSkeletonJoint(skeleton, combo.JointType, sensor);
                Vector offsetCoord = GetImageCoordAfterOffset(combo, coord);
                if (offsetCoord.X != 0.0 || offsetCoord.Y == 0.0)
                {
                    DrawHardCodedBS(combo, coord, offsetCoord, dc);
                    dc.DrawImage(combo.Image.Source, new Rect(offsetCoord.X, offsetCoord.Y, combo.Image.Source.Width, combo.Image.Source.Height));
                }
            }
        }

        private Vector GetImageCoordAfterOffset(ImageJointCombo combo, Vector coord)
        {
            coord.X = coord.X + combo.Offset.X;
            coord.Y = coord.Y - combo.Offset.Y;
            return coord;
        }

        private void DrawHardCodedBS(ImageJointCombo combo, Vector coord, Vector offsetCoord, DrawingContext dc)
        {
            if (combo.Name == "BalloonHeadsRight" || combo.Name == "BalloonHeadsRight")
            {
                Vector start = new Vector(offsetCoord.X + (combo.Image.Width / 2), offsetCoord.Y + (combo.Image.Height/2));
                Vector end = coord;
                DrawLine(start, end, dc);
            }
        }

        private void DrawLine(Vector start, Vector end, DrawingContext dc)
        {
            Pen p = new Pen(Brushes.AntiqueWhite, 20);
            dc.DrawLine(p, new Point(start.X, start.Y), new Point(end.X, end.Y));
        }

        private Vector GetSkeletonJoint(Skeleton skeleton, JointType type, KinectSensor sensor)
        {
            foreach (Joint j in skeleton.Joints)
            {
                if (j.JointType == type)
                {
                    if ((null != sensor) && (null != sensor.ColorStream))
                    {
                        // This is used to map a skeleton point to the depth image location
                        if(j.Position.X != 0 || j.Position.Y != 0)
                        {
                            var colorPt = sensor.MapSkeletonPointToColor(j.Position, sensor.ColorStream.Format);
                            return new Vector(colorPt.X, colorPt.Y);
                        }
                    }
                }
            }
            return new Vector();
        }


        private bool _active = false;
        public bool Active
        {
            get
            {
                return _active;
            }
            set
            {
                if (value == false)
                {
                    _active = false;
                    foreach (ImageJointCombo combo in this.ImageCombos)
                    {
                        combo.Image.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

    }


}
