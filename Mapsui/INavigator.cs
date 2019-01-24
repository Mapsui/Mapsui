using System;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui
{
    public interface INavigator
    {
        /// <summary>
        /// Called each time one of the navigation methods is called
        /// </summary>
        EventHandler Navigated { get; set; }
        
        /// <summary>
        /// Navigate center of viewport to center of extent and change resolution
        /// </summary>
        /// <param name="extent">New extent for viewport to show</param>
        /// <param name="scaleMethod">Scale method to use to determin resolution</param>
        void NavigateTo(BoundingBox extent, ScaleMethod scaleMethod = ScaleMethod.Fit);

        /// <summary>
        /// Change both center and resolution of the viewport
        /// </summary>
        /// <param name="center">The new center</param>
        /// <param name="resolution">The new resolution</param>
        void NavigateTo(Point center, double resolution);

        /// <summary>
        /// Change resolution of viewport
        /// </summary>
        /// <param name="resolution">New resolution to use</param>
        void ZoomTo(double resolution);
        
        /// <summary>
        /// Change center of viewport
        /// </summary>
        /// <param name="center">New center point of viewport</param>
        void CenterOn(Point center);

        /// <summary>
        /// Change center of viewport to X/Y coordinates
        /// </summary>
        /// <param name="x">X value of the new center</param>
        /// <param name="y">Y value of the new center</param>
        void CenterOn(double x, double y);
        
        /// <summary>
        /// Change rotation of viewport
        /// </summary>
        /// <param name="rotation">New rotation in degrees of viewport></param>
        void RotateTo(double rotation);

        void ZoomOut();

        void ZoomIn();

        void ZoomIn(Point centerOfZoom);
        
        void ZoomOut(Point centerOfZoom);

        void NavigateToFullEnvelope(ScaleMethod scaleMethod = ScaleMethod.Fill);

        void ZoomTo(double resolution, Point centerOfZoom);
    }
}
