using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Kinect.Toolkit.FaceTracking
{
    public class FaceInFocusEventHandler: EventArgs
    {

        public FaceInFocusEventHandler(IEnumerable<Microsoft.Kinect.Skeleton> skeltons)
            : base()
        {
            Skeletons=skeltons.ToList().AsReadOnly();
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Microsoft.Kinect.Skeleton> Skeletons
        {
            get;
            private set;
        }
    }
}
