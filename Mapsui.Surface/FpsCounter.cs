// Copyright 2008 - Paul den Dulk (Geodan)
// 
// This file is part of SharpMap.
// SharpMap is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// SharpMap is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with SharpMap; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Mapsui.Surface
{
  /// <remarks>Copied this code from some Silverlight game blog. PDD</remarks>
  class FpsCounter : DependencyObject
  {
    #region DependencyProperty

    private static readonly DependencyProperty FpsProperty = 
      System.Windows.DependencyProperty.Register(
      "Fps", typeof(int), typeof(FpsCounter));

    public int Fps
    {
      get { return (int)GetValue(FpsProperty); }
      set { SetValue(FpsProperty, value); }
    }

    #endregion

    #region Fields

    private double elapsed;
    private double totalElapsed;
    private int lastTick;
    private int currentTick;
    private int frameCount;
    private double frameCountTime;
   
    #endregion

    #region Methods

    public FpsCounter()
    {
      this.lastTick = Environment.TickCount;
    }

    public void FramePlusOne()
    {
      this.currentTick = Environment.TickCount;
      this.elapsed = (double)(this.currentTick - this.lastTick) / 1000.0;
      this.totalElapsed += this.elapsed;
      this.lastTick = this.currentTick;

      frameCount++;
      frameCountTime += elapsed;
      if (frameCountTime >= 1.0)
      {
        frameCountTime -= 1.0;
        Fps = frameCount;
        frameCount = 0;
      }
    }

    #endregion
  }
}
