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

namespace Mapsui.Windows
{
    public class FpsCounter : DependencyObject, INotifyPropertyChanged
    {
        #region Fields

        private double elapsed;
        private int lastTick;
        private int currentTick;
        private int frameCount;
        private double frameCountTime;
        
        #endregion

        #region DependencyProperties

        private static readonly DependencyProperty FpsProperty = DependencyProperty.Register(
          "Fps", typeof(int), typeof(FpsCounter), new PropertyMetadata(0));


        #endregion

        #region Properties

        public int Fps
        {
            get { return (int)GetValue(FpsProperty); }
            set
            {
                SetValue(FpsProperty, value);
                OnPropertyChanged("Fps");
            }
        }

        #endregion

        #region Methods

        public FpsCounter()
        {
            lastTick = Environment.TickCount;
        }

        public void FramePlusOne()
        {
            currentTick = Environment.TickCount;
            elapsed = (currentTick - lastTick) / 1000.0;
            lastTick = currentTick;

            frameCount++;
            frameCountTime += elapsed;
            if (frameCountTime >= 1.0)
            {
                frameCountTime -= 1.0;
                Fps = frameCount;
                frameCount = 0;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
