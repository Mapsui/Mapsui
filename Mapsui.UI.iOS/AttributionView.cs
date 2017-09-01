using System;
using System.Text;
using System.Collections.Generic;
using Mapsui.Layers;
using UIKit;
using CoreGraphics;
using Foundation;

namespace Mapsui.UI.iOS
{
	public sealed class AttributionView: UIView
	{
		readonly UIStackView stackView;

		public AttributionView ()
		{
			stackView = new UIStackView ();
			stackView.TranslatesAutoresizingMaskIntoConstraints = false;
			stackView.Axis = UILayoutConstraintAxis.Vertical;
			stackView.Alignment = UIStackViewAlignment.Fill;
			stackView.Distribution = UIStackViewDistribution.Fill;

			AddSubview (stackView);

			AddConstraints (new NSLayoutConstraint [] {
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Left, NSLayoutRelation.Equal, stackView, NSLayoutAttribute.Left, 1.0f, -4.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Right, NSLayoutRelation.Equal, stackView, NSLayoutAttribute.Right, 1.0f, 4.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Top, NSLayoutRelation.Equal, stackView, NSLayoutAttribute.Top, 1.0f, -4.0f),
				NSLayoutConstraint.Create(this, NSLayoutAttribute.Bottom, NSLayoutRelation.Equal, stackView, NSLayoutAttribute.Bottom, 1.0f, 4.0f),
			});

			Font = UIFont.PreferredFootnote;
		}

		public UIFont Font { get; set; }

		public void Populate (ICollection<ILayer> layers, CGRect parentFrame)
		{
			Clear ();
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
						var label = new UILabel ();
						label.BackgroundColor = UIColor.Clear;
						label.TranslatesAutoresizingMaskIntoConstraints = false;
						label.Text = layer.Attribution.Text;
						label.Font = Font;
						rowView = label;
					}
					else
					{
						var button = new UIButton (UIButtonType.Custom);
						button.TranslatesAutoresizingMaskIntoConstraints = false;
						button.TouchUpInside += (sender, e) => UIApplication.SharedApplication.OpenUrl(url);
						button.SetTitle (string.Format ("{0} ({1})", layer.Attribution.Text, layer.Attribution.Url), UIControlState.Normal);
						button.SetTitleColor (TintColor, UIControlState.Normal);
						button.Font = Font;
						rowView = button;
					}
					stackView.AddArrangedSubview (rowView);
				}
			}
			Hidden = stackView.Subviews.Length == 0;
		}

		public void Clear ()
		{
			foreach (var subview in stackView.Subviews)
			{
				subview.RemoveFromSuperview ();
			}
			Hidden = true;
		}
	}
}