using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using BruTile.Predefined;
using Mapsui.Geometries;
using Mapsui.Layers;

namespace Mapsui.Samples.Wpf.Mvvm
{
    public class ViewModel : INotifyPropertyChanged
    {
        private Map _map;
        public Map Map
        {
            get => _map;
            set
            {
                _map = value;
                if (value == null) return;
                _map.CRS = "EPSG:3857";
                _map?.ClearCache();
            }
        }
        public INavigator Navigator { get; set; }
        public IViewport Viewport { get; set; }

        public ICommand AddBingAerialLayerCommand { get; set; }
        public ICommand StartAnimationCommand { get; set; }
        private readonly Random _random = new Random();
        private readonly DispatcherTimer _dispatcherTimer = new DispatcherTimer();
        public string StartAnimationButtonText { get; set; }

        public ViewModel()
        {
            AddBingAerialLayerCommand = new RelayCommand(OnAddBingAerialLayer);
            StartAnimationCommand = new RelayCommand(OnStartAnimations);
            _dispatcherTimer.Tick += TimerTick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 5);
            StartAnimationButtonText = "Start animation";
            OnPropertyChanged(nameof(StartAnimationButtonText));
        }

        private void OnAddBingAerialLayer(object obj)
        {
            _map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial)));
            Navigator.NavigateTo(_map.Resolutions[5]);
        }

        private void OnStartAnimations(object obj)
        {
            if (_dispatcherTimer.IsEnabled)
            {
                _dispatcherTimer.Stop();
                StartAnimationButtonText = "Start animation";
            }
            else
            {
                _dispatcherTimer.Start();
                StartAnimationButtonText = "Stop animation";
            }
            OnPropertyChanged(nameof(StartAnimationButtonText));
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Navigator.NavigateTo(new Point(_random.Next((int)_map.Envelope.MinX, (int)_map.Envelope.MaxX), _random.Next((int)_map.Envelope.MinY, (int)_map.Envelope.MaxY)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;

        private readonly Predicate<object> _canExecute;

        public event EventHandler CanExecuteChanged;

        private static bool DefaultCanExecute(object parameter)
        {
            return true;
        }

        public RelayCommand(Action<object> execute):
            this(execute, DefaultCanExecute)
        { }
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute.Invoke(parameter);
        }
    }
}
