using System;
using System.Threading;

namespace Mapsui.UI.iOS
{
	public class FpsCounter
	{
		private double _elapsed;
		private int _lastTick;
		private int _currentTick;
		private int _frameCount;
		private double _frameCountTime;
        double _start;

		public int Fps;

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

		public void CapFrameRate(double fps)
		{
			var wait = 1000 / fps;
			var diff = Environment.TickCount - _start;

			if (diff < wait) {
				var waitTime = (wait - diff);
				Thread.Sleep ((int)waitTime);
				Console.WriteLine ("wait time = " + (int)waitTime );
			}

			_start = Environment.TickCount;
		}
	}
}