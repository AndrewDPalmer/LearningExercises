using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KinectHandler
{
    public class ZoomLevel
    {
        public double Zoom { get { return _currentZoom * _zoomFactor; } set { _currentZoom = value; } }

        private double _currentZoom = 3;
        private double _maxZoom = 4.0;
        private double _minZoom = 0.0;
        private double _zoomFactor = 500;

        public ZoomLevel(double zoom)
        {
            _currentZoom = zoom;
        }

        public void ZoomOut()
        {
            _currentZoom = _currentZoom - (_maxZoom * 0.001);
            if (_currentZoom < _minZoom)
            {
                _currentZoom = _minZoom;
            }
        }

        public void ZoomIn()
        {
            _currentZoom = _currentZoom + (_maxZoom * 0.001);
            if (_currentZoom > _maxZoom)
            {
                _currentZoom = _maxZoom;
            }
        }
    }
}
