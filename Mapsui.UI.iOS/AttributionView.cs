using System;
using System.Collections.Generic;
using Mapsui.Layers;
using UIKit;
using CoreGraphics;
using Foundation;

namespace Mapsui.UI.iOS
{
	public sealed class AttributionView: UIView
	{
		readonly UIStackView _stackView;

		public AttributionView ()
		{
		    _stackView = new UIStackView
		    {
		        TranslatesAutoresizingMaskIntoConstraints = false,
		        Axis = UILayoutConstraintAxis.Vertical,
		        Alignment = UIStackViewAlignment.Fill,
		        Distribution = UIStackViewDistribution.Fill
		    };

		    AddSubview (_stackView);

			AddConstraints (new [] {
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Left, NSLayoutRelation.Equal, _stackView, NSLayoutAttribute.Left, 1.0f, -1.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Right, NSLayoutRelation.Equal, _stackView, NSLayoutAttribute.Right, 1.0f, 1.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, _stackView, NSLayoutAttribute.Top, 1.0f, -1.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, _stackView, NSLayoutAttribute.Bottom, 1.0f, 1.0f),
			});

			Font = UIFont.PreferredFootnote;
		}

		public UIFont Font { get; set; }

		public void Populate (ICollection<ILayer> layers, CGRect parentFrame)
		{
			Clear();
			foreach (var layer in layers)
			{
				if (!string.IsNullOrEmpty (layer.Attribution.Text))
				{
					NSUrl url;
					try
					{
						url = new NSUrl (layer.Attribution.Url);
					}
					catch (Exception)
					{
						url = null;
					}
					UIView rowView;
					if (url == null || string.IsNullOrEmpty(layer.Attribution.Url))
					{
					    var label = new UILabel
					    {
					        BackgroundColor = UIColor.Clear,
					        TranslatesAutoresizingMaskIntoConstraints = false,
					        Text = layer.Attribution.Text,
					        Font = Font
                        };
                        rowView = label;
					}
					else
					{
					    var button = new UIButton(UIButtonType.Custom)
					    {
					        TranslatesAutoresizingMaskIntoConstraints = false,
                            Font = Font
                        };
					    button.TouchUpInside += (sender, e) => UIApplication.SharedApplication.OpenUrl(url);
						button.SetTitle ($"{layer.Attribution.Text}", UIControlState.Normal);
						button.SetTitleColor (TintColor, UIControlState.Normal);
						rowView = button;
					}
					_stackView.AddArrangedSubview (rowView);
				}
			}
			Hidden = _stackView.Subviews.Length == 0;
		}

		public void Clear ()
		{
			foreach (var subview in _stackView.Subviews)
			{
				subview.RemoveFromSuperview ();
			}
			Hidden = true;
		}
	}
}