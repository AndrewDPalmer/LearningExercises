using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FaceTrackingShane
{
    public class FaceImageData
    {
        public readonly string fileName;
        public readonly int top;
        public readonly int left;
        public readonly int width;
        public readonly int height;

        public FaceImageData(string fileName, int left, int top, int width, int height)
        {
            this.fileName = fileName;
            this.top = top;
            this.left = left;
            this.width = width;
            this.height = height;
        }
    }
}
