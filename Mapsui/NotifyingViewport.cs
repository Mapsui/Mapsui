using System.ComponentModel;
using Mapsui.Geometries;

namespace Mapsui
{
    public class NotifyingViewport : IViewport
    {
        private readonly Viewport _viewport = new Viewport();

        public event PropertyChangedEventHandler PropertyChanged;

        public double RenderResolution
        {
            get { return _viewport.RenderResolution; }
        }

        public double RenderScaleFactor
        {
            set { _viewport.RenderScaleFactor = value; }
        }
        
        public Point Center
        {
            set { 
                _viewport.Center = value;
                RaisePropertyChanged("Center");
            }
        }

        public double CenterX
        {
            get { return _viewport.CenterX; }
            set
            {
                _viewport.CenterX = value;
                RaisePropertyChanged("CenterX");
            }
        }

        public double CenterY
        {
            get { return _viewport.CenterY; }
            set { 
                _viewport.CenterY = value;
                RaisePropertyChanged("CenterY");
            }
        }

        public double Width
        {
            get { return _viewport.Width; }
            set { 
                _viewport.Width = value;
                RaisePropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _viewport.Height; }
            set
            {
                _viewport.Height = value;
                RaisePropertyChanged("Height");
            }
        }

        public double Resolution
        {
            get { return _viewport.Resolution; }
            set
            {
                _viewport.Resolution = value;
                RaisePropertyChanged("Resolution");
            }
        }

        public BoundingBox Extent
        {
            get { return _viewport.Extent; }
        }

        public Point ScreenToWorld(double x, double y)
        {
            return _viewport.ScreenToWorld(x, y);
        }

        public Point ScreenToWorld(Point point)
        {
            return _viewport.ScreenToWorld(point);
        }

        public Point WorldToScreen(double x, double y)
        {
            return _viewport.WorldToScreen(x, y);
        }

        public Point WorldToScreen(Point point)
        {
            return _viewport.WorldToScreen(point);
        }

        public void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY, double deltaScale = 1)
        {
            _viewport.Transform(screenX, screenY, previousScreenX, previousScreenY, deltaScale);
        }

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
