﻿/*
Copyright (c) 2017, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System;
using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.Platform;
using MatterHackers.Agg.UI;
using MatterHackers.VectorMath;

namespace MatterHackers.MatterControl.CustomWidgets
{
	public class ExpandCheckboxButton : FlowLayoutWidget, ICheckbox
	{
		public event EventHandler CheckedStateChanged;

		private IconButton imageButton;

		private ImageBuffer arrowRight;

		private ImageBuffer arrowDown;

		private TextWidget textWidget;

		private bool _expandable = true;

		private bool _checked;

		public ExpandCheckboxButton(string text, ThemeConfig theme, int pointSize = 11, bool expandable = true)
		{
			arrowRight = AggContext.StaticData.LoadIcon("fa-angle-right_12.png", theme.InvertIcons);
			arrowDown = AggContext.StaticData.LoadIcon("fa-angle-down_12.png", theme.InvertIcons);

			imageButton = new IconButton(arrowRight, theme)
			{
				MinimumSize = new Vector2(theme.ButtonHeight, theme.ButtonHeight),
				VAnchor = VAnchor.Center,
				Selectable = false,
				Enabled = expandable
			};
			this.AddChild(imageButton);

			_expandable = expandable;

			this.AddChild(textWidget = new TextWidget(text, pointSize: pointSize, textColor: theme.TextColor)
			{
				VAnchor = VAnchor.Center,
				AutoExpandBoundsToText = true
			});

			foreach(var child in this.Children)
			{
				child.Selectable = false;
			}
		}

		internal void SetIconMargin(BorderDouble margin)
		{
			imageButton.Margin = margin;
		}

		public bool AlwaysShowArrow { get; set; }

		public bool Expandable
		{
			get => _expandable;
			set
			{
				if (_expandable != value)
				{
					_expandable = value;

					imageButton.SetIcon(_expandable || this.AlwaysShowArrow ? arrowRight : new ImageBuffer());
					imageButton.Enabled = _expandable;

					this.MinimumSize = new Vector2((_expandable) ? this.MinimumSize.X : 10, this.MinimumSize.Y);
				}
			}
		}

		public bool ShowIcon
		{
			get => imageButton.Visible;
			set
			{
				if (!value)
				{
					imageButton.Parent.MinimumSize = new Vector2(0, imageButton.Height);
				}

				imageButton.Visible = value;
			}
		}

		public override string Text
		{
			get => textWidget.Text;
			set => textWidget.Text = value;
		}

		public void OnCheckChanged()
		{
			CheckedStateChanged?.Invoke(this, null);
		}

		protected override void OnClick(MouseEventArgs mouseEvent)
		{
			if (this.Expandable)
			{
				UiThread.RunOnIdle(() => this.Checked = !this.Checked);
			}

			base.OnClick(mouseEvent);
		}

		public bool Checked
		{
			get => _checked;
			set
			{
				if (_checked != value)
				{
					_checked = value;

					if (this.Expandable)
					{
						imageButton.SetIcon(value ? arrowDown : arrowRight);
					}

					Invalidate();

					OnCheckChanged();
				}
			}
		}
	}
}
