using System.ComponentModel;

namespace Mapsui.Geometries
{
    class NotifyingPoint : Point, INotifyPropertyChanged
    {
        public new double X 
        {
            get { return base.X; }
            set
            {
                base.X = value; 
                OnPropertyChanged("X");                
            }
        }

        public new double Y
        {
            get { return base.Y; }
            set
            {
                base.Y = value; 
                OnPropertyChanged("Y");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}