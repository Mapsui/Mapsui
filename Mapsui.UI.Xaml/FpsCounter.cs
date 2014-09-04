// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of Mapsui.
// Mapsui is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Mapsui is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with Mapsui; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.ComponentModel;
using System.Windows;
#if NETFX_CORE
using Windows.UI.Xaml;
#endif

namespace Mapsui.UI.Xaml
{
    public class FpsCounter : DependencyObject, INotifyPropertyChanged
    {
        private double _elapsed;
        private int _lastTick;
        private int _currentTick;
        private int _frameCount;
        private double _frameCountTime;

        public event PropertyChangedEventHandler PropertyChanged;

        private static readonly DependencyProperty FpsProperty = DependencyProperty.Register(
          "Fps", typeof(int), typeof(FpsCounter), new PropertyMetadata(0));

        public int Fps
        {
            get { return (int)GetValue(FpsProperty); }
            set
            {
                SetValue(FpsProperty, value);
                OnPropertyChanged("Fps");
            }
        }

        public FpsCounter()
        {
            _lastTick = Environment.TickCount;
        }

        public void FramePlusOne()
        {
            _currentTick = Environment.TickCount;
            _elapsed = (_currentTick - _lastTick) / 1000.0;
            _lastTick = _currentTick;

            _frameCount++;
            _frameCountTime += _elapsed;
            if (_frameCountTime >= 1.0)
            {
                _frameCountTime -= 1.0;
                Fps = _frameCount;
                _frameCount = 0;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}