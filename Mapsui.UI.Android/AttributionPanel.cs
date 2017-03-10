using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Mapsui.Layers;

namespace Mapsui.UI.Android
{
    public class AttributionPanel : LinearLayout
    {
        public AttributionPanel(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Initialize();
        }

        public AttributionPanel(Context context) : base(context)
        {
            Initialize();
        }

        public AttributionPanel(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize();
        }

        public AttributionPanel(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Initialize();
        }

        private void Initialize()
        {
            LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
            Orientation = Orientation.Vertical;
            SetGravity(GravityFlags.CenterHorizontal);
        }

        public void Populate(IEnumerable<ILayer> layers)
        {
            if (ChildCount > 0) RemoveAllViews();

            var count = 0;

            foreach (var layer in layers)
            {
                if (!string.IsNullOrEmpty(layer.Attribution?.Text))
                {
                    var textView = new TextView(Context)
                    {
                        Text = layer.Attribution.Text + " " + layer.Attribution.Url,
                        LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent,
                            ViewGroup.LayoutParams.WrapContent)
                    };
                    textView.SetTextColor(Color.Black);
                    UpdateSize(textView);
                    MoveDown(textView, count * textView.Height);
                    AddView(textView);
                    count++;
                }
                
            }
            PostInvalidate();
        }

        private void MoveDown(View view, int offsetX)
        {
            view.Top = view.Top + offsetX;
            view.Bottom =view.Bottom + offsetX;
        }

        private static void UpdateSize(View view)
        {
            // I created this method because I don't understand what I'm doing
            view.Measure(0, 0);
            view.Right = view.Left + view.MeasuredWidth;
            view.Bottom = view.Top + view.MeasuredHeight;
        }

        public void Clear()
        {
            if (ChildCount > 0)
                RemoveAllViews();
        }
    }
}