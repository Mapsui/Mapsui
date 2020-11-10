using Mapsui.Geometries;
using Mapsui.Utilities;
using System;

namespace Mapsui
{
    [Obsolete("Please just use normal Navigator, it has animation capability built in now", true)]
    public class AnimatedNavigator : INavigator
    {
        public EventHandler<ChangeType> Navigated { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void FlingWith(double velocityX, double velocityY, long maxDuration)
        {
            throw new NotImplementedException();
        }

        public void CenterOn(Point center, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void CenterOn(double x, double y, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void NavigateTo(Point center, double resolution, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void RotateTo(double rotation, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void StopRunningAnimation()
        {
            throw new NotImplementedException();
        }

        public void ZoomIn(long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void ZoomIn(Point centerOfZoom, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void ZoomOut(long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void ZoomOut(Point centerOfZoom, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void ZoomTo(double resolution, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void ZoomTo(double resolution, Point centerOfZoom, long duration = 0, Easing easing = null)
        {
            throw new NotImplementedException();
        }

        public void UpdateAnimations()
        {
            throw new NotImplementedException();
        }
    }
}
