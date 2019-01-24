using System;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    public class Navigator : INavigator
    {
        private readonly Map _map;
        private readonly IViewport _viewport;

        public EventHandler Navigated { get; set; } 

        public Navigator(Map map, IViewport viewport)
        {
            _map = map;
            _viewport = viewport;
        }

        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determine resolution</param>
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            if (extent == null) return;

            var resolution = ZoomHelper.DetermineResolution(
                extent.Width, extent.Height, _viewport.Width, _viewport.Height, scaleMethod);
            _viewport.SetResolution(resolution);

            _viewport.SetCenter(extent.Centroid);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        public void ZoomTo(double resolution)
        {
            _viewport.SetResolution(resolution);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        public void CenterOn(Point center)
        {
            _viewport.SetCenter(center);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <inheritdoc />
        public void NavigateTo(Point center, double resolution)
        {
            _viewport.SetCenter(center);
            _viewport.SetResolution(resolution);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        public void CenterOn(double x, double y)
        {
            _viewport.SetCenter(x, y);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        public void RotateTo(double rotation)
        {
            _viewport.SetRotation(rotation);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomIn()
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);
            _viewport.SetResolution(resolution);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomOut()
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);
            _viewport.SetResolution(resolution);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomIn(Point centerOfZoom)
        {
            var resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);
            ZoomTo(resolution, centerOfZoom);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomOut(Point centerOfZoom)
        {
            var resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);
            ZoomTo(resolution, centerOfZoom);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill)
        {
            NavigateTo(_map.Envelope, scaleMethod);

            Navigated?.Invoke(this, EventArgs.Empty);
        }

        public void ZoomTo(double resolution, Point centerOfZoom)
        {
            // 1) Temporarily center on the center of zoom
            _viewport.SetCenter(_viewport.ScreenToWorld(centerOfZoom));

            // 2) Then zoom 
            _viewport.SetResolution(resolution);

            // 3) Then move the temporary center of the map back to the mouse position
            _viewport.SetCenter(_viewport.ScreenToWorld(
                _viewport.Width - centerOfZoom.X,
                _viewport.Height - centerOfZoom.Y));

            Navigated?.Invoke(this, EventArgs.Empty);
        }
    }
}
