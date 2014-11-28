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

        public double RenderResolutionMultiplier
        {
            set { _viewport.RenderResolutionMultiplier = value; }
        }

        public Point Center
        {
            get { return _viewport.Center;  }
            set {
                if (Equals(_viewport.Center, value)) return; 
                _viewport.Center = value;
                OnPropertyChanged("Center");
            }
        }

        public double Width
        {
            get { return _viewport.Width; }
            set {
                if (_viewport.Width == value) return; 
                _viewport.Width = value;
                OnPropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _viewport.Height; }
            set
            {
                if (_viewport.Height == value) return; 
                _viewport.Height = value;
                OnPropertyChanged("Height");
            }
        }

        public double Rotation
        {
            get { return _viewport.Rotation; }
            set
            {
                if (_viewport.Rotation == value) return; 
                _viewport.Rotation = value;
                OnPropertyChanged("Rotation");
                OnPropertyChanged("IsRotated");
            }
        }

        public bool IsRotated
        {
            get { return _viewport.IsRotated; }
        }

        public double Resolution
        {
            get { return _viewport.Resolution; }
            set
            {
                if (_viewport.Resolution == value) return; 
                _viewport.Resolution = value;
                OnPropertyChanged("Resolution");
            }
        }

        public BoundingBox Extent
        {
            get { return _viewport.Extent; }
        }

        public Quad WindowExtent
        {
            get { return _viewport.WindowExtent; }
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

        public Point WorldToScreenUnrotated(double x, double y)
        {
            return _viewport.WorldToScreenUnrotated(x, y);
        }

        public Point WorldToScreen(Point point)
        {
            return _viewport.WorldToScreen(point);
        }

        public Point WorldToScreenUnrotated(Point point)
        {
            return _viewport.WorldToScreenUnrotated(point);
        }

        public void Transform(double screenX, double screenY, double previousScreenX, double previousScreenY, double deltaScale = 1)
        {
            _viewport.Transform(screenX, screenY, previousScreenX, previousScreenY, deltaScale);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
