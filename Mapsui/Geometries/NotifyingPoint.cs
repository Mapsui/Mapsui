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

        /// <summary>
        ///     Checks whether this instance is spatially equal to the Point 'o'
        /// </summary>
        /// <param name="p">Point to compare to</param>
        /// <returns></returns>
        public override bool Equals(Point p)
        {
            // ReSharper disable CompareOfFloatsByEqualityOperator
            return (p != null) && (p.X == X) && (p.Y == Y) && (IsEmpty() == p.IsEmpty());
            // ReSharper restore CompareOfFloatsByEqualityOperator
        }
    }
}