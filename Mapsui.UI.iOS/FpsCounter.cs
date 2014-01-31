using System;
using MonoTouch.CoreAnimation;
using System.Windows;
using System.ComponentModel;
using System.Threading;
using MonoTouch.Foundation;

namespace Mapsui.UI.iOS
{
	public class FpsCounter
	{
		private double elapsed;
		private int lastTick;
		private int currentTick;
		private int frameCount;
		private double frameCountTime;

		public int Fps;

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

		double start = 0;

		public void CapFrameRate(double fps)
		{
			var wait = 1000 / fps;
			var diff = Environment.TickCount - start;

			if (diff < wait) {
				var waitTime = (wait - diff);
				Thread.Sleep ((int)waitTime);
				Console.WriteLine ("wait time = " + (int)waitTime );
			}

			start = Environment.TickCount;
		}
	}
}