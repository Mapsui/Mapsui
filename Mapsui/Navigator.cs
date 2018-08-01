using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    public class Navigator : INavigator
    {
        private readonly Map _map;
        private readonly IViewport _viewport;

        public Navigator(Map map, IViewport viewport)
        {
            _map = map;
            _viewport = viewport;
        }

        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determin resolution</param>
        public void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit)
        {
            if (extent == null) return;
            _viewport.SetResolution(ZoomHelper.DetermineResolution(
                extent.Width, extent.Height, _viewport.Width, _viewport.Height, scaleMethod));
            _viewport.SetCenter(extent.Centroid);
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        public void NavigateTo(double resolution)
        {
            _viewport.SetResolution(resolution);
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        public void NavigateTo(Point center)
        {
            _viewport.SetCenter(center);
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        /// <inheritdoc />
        public void NavigateTo(Point center, double resolution)
        {
            _viewport.SetCenter(center);
            _viewport.SetResolution(resolution);
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        public void NavigateTo(double x, double y)
        {
            _viewport.SetCenter(x, y);
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        public void RotateTo(double rotation)
        {
            _viewport.SetRotation(rotation);
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        public void ZoomIn()
        {
            _viewport.SetResolution(ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution));
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        public void ZoomOut()
        {
            _viewport.SetResolution(ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution));
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }

        public void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill)
        {
            NavigateTo(_map.Envelope, scaleMethod);
            _map.RefreshData(_viewport.Extent, _viewport.Resolution, true);
        }
    }
}
