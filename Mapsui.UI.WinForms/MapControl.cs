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

using Mapsui.Rendering.Gdi;
using Mapsui.Fetcher;
using Mapsui.Utilities;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mapsui.UI.WinForms
{
    public class MapControl : Control
    {
        #region Fields

        private Map _map;
        private readonly Viewport _viewport = new Viewport();
        private string _errorMessage;
        private Bitmap _buffer;
        private Graphics _bufferGraphics;
        private readonly Brush _whiteBrush = new SolidBrush(Color.White);
        private Geometries.Point _mousePosition;
            //Indicates that a redraw is needed. This often coincides with 
            //manipulation but not in the case of new data arriving.
        private bool _viewInitialized;
        private readonly GdiMapRenderer _renderer = new GdiMapRenderer();

        #endregion

        public event EventHandler ErrorMessageChanged;


        #region Properties

        public Viewport Transform
        {
            get { return _viewport; }
        }

        public Map Map
        {
            get
            {
                return _map;
            }
            set
            {
                var temp = _map;
                _map = null;

                if (temp != null)
                {
                    temp.DataChanged -= MapDataChanged;
                    temp.Dispose();
                }

                _map = value;
                _map.DataChanged += MapDataChanged;

                ViewChanged(true);
                Invalidate();
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
            _viewport.Resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);
            ViewChanged(true);
            Invalidate();
        }

        public void ZoomIn(PointF mapPosition)
        {
                // When zooming in we want the mouse position to stay above the same world coordinate.
                // We do that in 3 steps.

                // 1) Temporarily center on where the mouse is
            _viewport.Center = _viewport.ScreenToWorld(mapPosition.X, mapPosition.Y);

                // 2) Then zoom 
            _viewport.Resolution = ZoomHelper.ZoomIn(_map.Resolutions, _viewport.Resolution);

                // 3) Then move the temporary center back to the mouse position
            _viewport.Center = _viewport.ScreenToWorld(
              _viewport.Width - mapPosition.X,
              _viewport.Height - mapPosition.Y);

            ViewChanged(true);
            Invalidate();
        }

        public void ZoomOut()
        {
            _viewport.Resolution = ZoomHelper.ZoomOut(_map.Resolutions, _viewport.Resolution);
            ViewChanged(true);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!_viewInitialized) InitializeView();
            if (!_viewInitialized) return; //initialize in the line above failed. 
            
            base.OnPaint(e);

            //Reset background
            _bufferGraphics.FillRectangle(_whiteBrush, 0, 0, _buffer.Width, _buffer.Height);
            
            //Render to the buffer
            _renderer.Graphics = _bufferGraphics;
            _renderer.Render(_viewport, _map.Layers);
            
            //Render the buffer to the control
            e.Graphics.DrawImage(_buffer, 0, 0);
        }

        private void ViewChanged(bool changeEnd)
        {
            if (_map != null)
            {
                _map.ViewChanged(changeEnd);
            }
        }

        private void DataChanged(object sender, DataChangedEventArgs e)
        {
            if (e.Error == null && e.Cancelled == false)
            {
                Invalidate();
            }
            else if (e.Cancelled)
            {
                _errorMessage = "Cancelled";
                OnErrorMessageChanged();
            }
            else if (e.Error is System.Net.WebException)
            {
                _errorMessage = "WebException: " + e.Error.Message;
                OnErrorMessageChanged();
            }
            else if (e.Error == null)
            {
                _errorMessage = "Unknown Exception";
                OnErrorMessageChanged();
            }
            else
            {
                _errorMessage = "Exception: " + e.Error.Message;
                OnErrorMessageChanged();
            }
        }

        private void MapControl_MouseDown(object sender, MouseEventArgs e)
        {
            _mousePosition = new Geometries.Point(e.X, e.Y);
        }

        private void MapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_mousePosition == null) return;
                var newMousePosition = new Geometries.Point(e.X, e.Y);
                MapTransformHelpers.Pan(_viewport, newMousePosition, _mousePosition);
                _mousePosition = newMousePosition;

                ViewChanged(false);
                Invalidate();
            }
        }

        private void MapControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_mousePosition == null) return;
                var newMousePosition = new Geometries.Point(e.X, e.Y);
                MapTransformHelpers.Pan(_viewport, newMousePosition, _mousePosition);
                _mousePosition = newMousePosition;

                ViewChanged(true);
                Invalidate();
            }
        }

        private void MapControl_Resize(object sender, EventArgs e)
        {
            if (Width == 0) return;
            if (Height == 0) return;

            _viewport.Width = Width;
            _viewport.Height = Height;

            if (_buffer == null || _buffer.Width != Width || _buffer.Height != Height)
            {
                _buffer = new Bitmap(Width, Height);
                _bufferGraphics = Graphics.FromImage(_buffer);
            }

            ViewChanged(true);
            Invalidate();
        }

        private void InitializeView()
        {
            if (double.IsNaN(Width) || Width == 0) return;
            if (_map == null || _map.Envelope == null || double.IsNaN(_map.Envelope.Width) || _map.Envelope.Width <= 0) return;
            if (_map.Envelope.GetCentroid() == null) return;

            _viewport.Center = _map.Envelope.GetCentroid();
            _viewport.Resolution = _map.Envelope.Width / Width;
            _viewInitialized = true;
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
            if (DesignMode) base.OnPaintBackground(e);
            //by overriding this method and not calling the base class implementation 
            //we prevent flickering.
        }
    }
}
