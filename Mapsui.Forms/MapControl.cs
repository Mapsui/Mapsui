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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston,min MA  02111-1307  USA 

using Mapsui.Rendering.GdiRendering;
using Mapsui.Fetcher;
using Mapsui.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mapsui.Forms
{
    public class MapControl : Control
    {
        #region Fields

        private Map map;
        private Viewport viewport = new Viewport();
        private string errorMessage;
        private Bitmap buffer;
        private Graphics bufferGraphics;
        private Brush whiteBrush = new SolidBrush(Color.White);
        private Mapsui.Geometries.Point mousePosition;
            //Set if the user manipulates the map. If this happens we will limit 
            //the time it takes to render a frame by simply cutting if off. 
            //This way the control is always reponsive.
        private bool isManipulated; 
            //Indicates that a redraw is needed. This often coincides with 
            //manipulation but not in the case of new data arriving.
        private bool isInvalidated; 
        private bool viewInitialized;
        private long startTicks;
        private const int MaxMiliseconds = 40;
            //Ticks are in units of 100 nanoseconds. I prefer to use miliseconds myself
            //1 tick = 100 nanoseconds (from documentation)
            //1 milisecond = 1000000 nanoseconds
            //1 milisecond = (1000000 / 100) = 10000 ticks.
        private const long MaxTicks = (long)MaxMiliseconds * 10000;
        private bool isCallingDoEvents;

        #endregion

        public event EventHandler ErrorMessageChanged;


        #region Properties

        public Viewport Transform
        {
            get { return viewport; }
        }

        public Map Map
        {
            get
            {
                return map;
            }
            set
            {
                var temp = map;
                map = null;

                if (temp != null)
                {
                    temp.DataChanged -= MapDataChanged;
                    temp.Dispose();
                }

                map = value;
                map.DataChanged += MapDataChanged;

                ViewChanged(true);
                InvalidateMap(false);
            }
        }

        void MapDataChanged(object sender, DataChangedEventArgs e)
        {
                //ViewChanged should not be called here. This would cause a loop
            BeginInvoke((Action) (() => DataChanged(sender, e)));
        }
        
        #endregion

        public MapControl()
        {
            Map = new Map();
            Resize +=  MapControl_Resize;
            MouseDown += MapControl_MouseDown;
            MouseMove += MapControl_MouseMove;
            MouseUp += MapControl_MouseUp;
            Disposed += MapControl_Disposed;
        }

        public void ZoomIn()
        {
            viewport.Resolution = ZoomHelper.ZoomIn(map.Resolutions, viewport.Resolution);
            ViewChanged(true);
            InvalidateMap(true);
        }

        public void ZoomIn(PointF mapPosition)
        {
                // When zooming in we want the mouse position to stay above the same world coordinate.
                // We do that in 3 steps.

                // 1) Temporarily center on where the mouse is
            viewport.Center = viewport.ScreenToWorld(mapPosition.X, mapPosition.Y);

                // 2) Then zoom 
            viewport.Resolution = ZoomHelper.ZoomIn(map.Resolutions, viewport.Resolution);

                // 3) Then move the temporary center back to the mouse position
            viewport.Center = viewport.ScreenToWorld(
              viewport.Width - mapPosition.X,
              viewport.Height - mapPosition.Y);

            ViewChanged(true);
            InvalidateMap(true);
        }

        public void ZoomOut()
        {
            viewport.Resolution = ZoomHelper.ZoomOut(map.Resolutions, viewport.Resolution);
            ViewChanged(true);
            InvalidateMap(true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (isCallingDoEvents) return;
            if (!viewInitialized) InitializeView();
            if (!viewInitialized) return; //initialize in the line above failed. 
            isManipulated = false;
            isInvalidated = false;
            
            base.OnPaint(e);

            //Reset background
            bufferGraphics.FillRectangle(whiteBrush, 0, 0, buffer.Width, buffer.Height);

            //set startTicks for use in AbortRender
            startTicks = DateTime.Now.Ticks;

            //Render to the buffer
            GdiMapRenderer.Render(bufferGraphics, new Viewport(viewport), map, AbortRender);
            
            //Render the buffer to the control
            e.Graphics.DrawImage(buffer, 0, 0);

            if (isInvalidated) Invalidate();
        }

        private bool AbortRender()
        {
            // When calling DoEvents we want all events to be called except 
            // OnPaint. This is prevented by checking on isCallingDoEvents
            isCallingDoEvents = true;
            Application.DoEvents();
            isCallingDoEvents = false;
            if (isManipulated && (DateTime.Now.Ticks - startTicks) > MaxTicks) return true;
            return false;
        }

        private void ViewChanged(bool changeEnd)
        {
            if (map != null)
            {
                map.ViewChanged(changeEnd, viewport.Extent, viewport.Resolution);
            }
        }

        private void DataChanged(object sender, DataChangedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                InvalidateMap(false);
            }
            else if (e.Cancelled)
            {
                errorMessage = "Cancelled";
                OnErrorMessageChanged();
            }
            else if (e.Error is System.Net.WebException)
            {
                errorMessage = "WebException: " + e.Error.Message;
                OnErrorMessageChanged();
            }
            else if (e.Error == null)
            {
                errorMessage = "Unknown Exception";
                OnErrorMessageChanged();
            }
            else
            {
                errorMessage = "Exception: " + e.Error.Message;
                OnErrorMessageChanged();
            }
        }

        private void MapControl_MouseDown(object sender, MouseEventArgs e)
        {
            mousePosition = new Mapsui.Geometries.Point(e.X, e.Y);
        }

        private void MapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mousePosition == null) return;
                var newMousePosition = new Mapsui.Geometries.Point(e.X, e.Y);
                MapTransformHelpers.Pan(viewport, newMousePosition, mousePosition);
                mousePosition = newMousePosition;

                ViewChanged(false);
                InvalidateMap(true);
            }
        }

        private void MapControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mousePosition == null) return;
                var newMousePosition = new Mapsui.Geometries.Point(e.X, e.Y);
                MapTransformHelpers.Pan(viewport, newMousePosition, mousePosition);
                mousePosition = newMousePosition;

                ViewChanged(true);
                InvalidateMap(true);
            }
        }

        private void MapControl_Resize(object sender, EventArgs e)
        {
            if (Width == 0) return;
            if (Height == 0) return;

            viewport.Width = Width;
            viewport.Height = Height;

            if (buffer == null || buffer.Width != Width || buffer.Height != Height)
            {
                buffer = new Bitmap(Width, Height);
                bufferGraphics = Graphics.FromImage(buffer);
            }

            ViewChanged(true);
            InvalidateMap(true);
        }

        private void InvalidateMap(bool isManipulated)
        {
            this.isManipulated = isManipulated;
            isInvalidated = true;
            Invalidate();
        }

        private void InitializeView()
        {
            if (double.IsNaN(Width) || Width == 0) return;
            if (map == null || map.Envelope == null || double.IsNaN(map.Envelope.Width) || map.Envelope.Width <= 0) return;
            if (map.Envelope.GetCentroid() == null) return;

            viewport.Center = map.Envelope.GetCentroid();
            viewport.Resolution = map.Envelope.Width / Width;
            viewInitialized = true;
            ViewChanged(true);
        }

        protected override void Dispose(bool disposing)
        {
            Map.Dispose();
            base.Dispose(disposing);
        }

        private void MapControl_Disposed(object sender, EventArgs e)
        {
            Map.Dispose();
        }

        protected void OnErrorMessageChanged()
        {
            if (ErrorMessageChanged != null) ErrorMessageChanged(this, null);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            //by overriding this method and not calling the base class implementation 
            //we prevent flickering.
        }
    }
}
