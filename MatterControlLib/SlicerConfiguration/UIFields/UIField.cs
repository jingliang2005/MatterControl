﻿/*
Copyright (c) 2017, Lars Brubaker, John Lewin
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
using MatterHackers.Agg.UI;

namespace MatterHackers.MatterControl.SlicerConfiguration
{
	public abstract class UIField
	{
		public event EventHandler<FieldChangedEventArgs> ValueChanged;

		protected readonly int ControlWidth = (int)(60 * GuiWidget.DeviceScale + .5);

		public void SetValue(string newValue, bool userInitiated)
		{
			string convertedValue = this.ConvertValue(newValue);

			if (this.Value != convertedValue)
			{
				this.Value = convertedValue;
				this.OnValueChanged(new FieldChangedEventArgs(userInitiated));
			}
			else if (newValue != convertedValue)
			{
				// If the validated value matches the current value, then UI element values were rejected and must be discarded
				this.OnValueChanged(new FieldChangedEventArgs(userInitiated));
			}
		}

		public string Value { get; private set; }

		public GuiWidget Content { get; protected set; }

		public string HelpText { get; set; }

		public string Name { get; set; }

		public SliceSettingsRow Row { get; internal set; }

		protected virtual string ConvertValue(string newValue)
		{
			return newValue;
		}

		public virtual void Initialize(int tabIndex)
		{
		}

		protected virtual void OnValueChanged(FieldChangedEventArgs fieldChangedEventArgs)
		{
			ValueChanged?.Invoke(this, fieldChangedEventArgs);
		}
	}
}
