﻿/*
Copyright (c) 2018, Lars Brubaker, John Lewin
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

using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.Agg.VertexSource;
using MatterHackers.Localizations;
using MatterHackers.MatterControl;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.PartPreviewWindow;
using MatterHackers.VectorMath;
using System;
using System.Collections.Generic;

namespace MatterControlLib.SetupWizard
{
	public class TourOverlay : GuiWidget
	{
		private string description;
		private int displayCount;
		private int displayIndex;
		private int nextLocationIndex;
		private Popover popover;
		private GuiWidget targetWidget;
		private ThemeConfig theme;
		private GuiWidget tourWindow;

		public TourOverlay(GuiWidget tourWindow, GuiWidget targetWidget, string description, ThemeConfig theme, int nextLocationIndex, int displayIndex, int displayCount)
		{
			this.tourWindow = tourWindow;
			this.nextLocationIndex = nextLocationIndex;
			this.theme = theme;
			this.targetWidget = targetWidget;
			this.description = description;
			this.displayIndex = displayIndex;
			this.displayCount = displayCount;

			this.HAnchor = HAnchor.Stretch;
			this.VAnchor = VAnchor.Stretch;
		}

		public static async void ShowLocation(GuiWidget window, int locationIndex, int direction = 1)
		{
			var tourLocations = await ApplicationController.Instance.LoadProductTour();

			if (locationIndex >= tourLocations.Count)
			{
				locationIndex -= tourLocations.Count;
			}

			// Find the widget on screen to show
			GuiWidget GetLocationWidget(ref int findLocationIndex, out int displayIndex2, out int displayCount2)
			{
				displayIndex2 = 0;
				displayCount2 = 0;

				int checkLocation = 0;
				GuiWidget tourLocationWidget = null;
				while (checkLocation < tourLocations.Count)
				{
					var foundChildren = window.FindDescendants(tourLocations[checkLocation].WidgetName);

					GuiWidget foundLocation = null;
					foreach (var widgetAndPosition in foundChildren)
					{
						if (widgetAndPosition.widget.ActuallyVisibleOnScreen())
						{
							foundLocation = widgetAndPosition.widget;
							// we have found a target that is visible on screen, count it up
							displayCount2++;
							break;
						}
					}

					checkLocation++;

					// if we have not found the target yet
					if (checkLocation >= findLocationIndex
						&& tourLocationWidget == null)
					{
						tourLocationWidget = foundLocation;
						// set the index to the count when we found the widget we want
						displayIndex2 = displayCount2 - 1;
						findLocationIndex = (checkLocation < tourLocations.Count) ? checkLocation : 0;
					}
				}

				return tourLocationWidget;
			}

			int displayIndex;
			int displayCount;
			GuiWidget targetWidget = GetLocationWidget(ref locationIndex, out displayIndex, out displayCount);

			if (targetWidget != null)
			{
				var tourOverlay = new TourOverlay(window,
					targetWidget,
					tourLocations[locationIndex].Description,
					ApplicationController.Instance.Theme,
					locationIndex + 1,
					displayIndex,
					displayCount);
				window.AddChild(tourOverlay);
			}
		}

		public override void OnDraw(Graphics2D graphics2D)
		{
			var dimRegion = new VertexStorage();
			dimRegion.MoveTo(LocalBounds.Left, LocalBounds.Bottom);
			dimRegion.LineTo(LocalBounds.Right, LocalBounds.Bottom);
			dimRegion.LineTo(LocalBounds.Right, LocalBounds.Top);
			dimRegion.LineTo(LocalBounds.Left, LocalBounds.Top);

			var targetBounds = this.GetTargetBounds();

			var targetRect = new VertexStorage();
			targetRect.MoveTo(targetBounds.Right, targetBounds.Bottom);
			targetRect.LineTo(targetBounds.Left, targetBounds.Bottom);
			targetRect.LineTo(targetBounds.Left, targetBounds.Top);
			targetRect.LineTo(targetBounds.Right, targetBounds.Top);

			var overlayMinusTargetRect = new CombinePaths(dimRegion, targetRect);
			graphics2D.Render(overlayMinusTargetRect, new Color(Color.Black, 180));

			base.OnDraw(graphics2D);

			graphics2D.Render(new Stroke(new RoundedRect(GetTargetBounds(), 0), 2), Color.White.WithAlpha(50));
			//graphics2D.Render(new Stroke(new RoundedRect(GetContentBounds(), 3), 4), theme.PrimaryAccentColor);
		}

		public override void OnKeyDown(KeyEventArgs keyEvent)
		{
			if (keyEvent.KeyCode == Keys.Escape)
			{
				this.Close();
			}

			if (keyEvent.KeyCode == Keys.Enter)
			{
				var topWindow = this.TopmostParent();
				this.Close();
				ShowLocation(topWindow, nextLocationIndex);
			}

			base.OnKeyDown(keyEvent);
		}

		public override void OnLoad(EventArgs args)
		{
			var column = new FlowLayoutWidget(FlowDirection.TopToBottom)
			{
				HAnchor = HAnchor.Absolute,
				VAnchor = VAnchor.Fit,
			};

			column.AddChild(new WrappedTextWidget(description, textColor: theme.TextColor, pointSize: theme.DefaultFontSize)
			{
				Margin = 5,
				HAnchor = HAnchor.Stretch
			});

			var buttonRow = new FlowLayoutWidget()
			{
				HAnchor = HAnchor.Stretch,
				Margin = new BorderDouble(0, 0, 0, 5)
			};
			column.AddChild(buttonRow);

			var prevButton = theme.CreateDialogButton("Prev".Localize());
			prevButton.Click += (s, e) =>
			{
				this.Close();
				ShowLocation(tourWindow, nextLocationIndex, -1);
			};
			buttonRow.AddChild(prevButton);

			buttonRow.AddChild(new HorizontalSpacer());

			buttonRow.AddChild(new TextWidget($"{displayIndex + 1} of {displayCount}", pointSize: theme.H1PointSize, textColor: theme.TextColor));

			buttonRow.AddChild(new HorizontalSpacer());

			var nextButton = theme.CreateDialogButton("Next".Localize());
			nextButton.Click += (s, e) =>
			{
				this.Close();
				ShowLocation(tourWindow, nextLocationIndex);
			};
			buttonRow.AddChild(nextButton);

			column.Size = new Vector2(250, column.Height);

			popover = new TourPopover(column, theme, targetWidget, this.GetTargetBounds());

			//popover.AddChild(column);
			this.AddChild(popover);

			this.Focus();

			base.OnLoad(args);
		}

		private RectangleDouble GetTargetBounds()
		{
			var childBounds = targetWidget.TransformToScreenSpace(targetWidget.LocalBounds);
			return this.TransformFromScreenSpace(childBounds);
		}
	}
}