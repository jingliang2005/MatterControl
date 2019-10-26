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

using System;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.PrintLibrary;
using MatterHackers.MatterControl.SlicerConfiguration;

namespace MatterHackers.MatterControl
{
	public class SelectPrinterProfilePage : SelectablePrinterPage
	{
		private Action<PrinterConfig> printerLoaded;

		public SelectPrinterProfilePage(string continueButtonText, Action<PrinterConfig> printerLoaded = null)
			: base(continueButtonText)
		{
			this.printerLoaded = printerLoaded;

			HardwareTreeView.CreatePrinterProfilesTree(rootPrintersNode, theme);
		}

		public SelectPrinterProfilePage(string continueButtonText) : base(continueButtonText)
		{
		}

		protected override void OnTreeNodeDoubleClicked(TreeNode treeNode)
		{
			if (treeNode.Tag is PrinterInfo printerInfo)
			{
				this.OnContinue(treeNode);
			}

			base.OnTreeNodeDoubleClicked(treeNode);
		}

		protected override void OnTreeNodeSelected(TreeNode selectedNode)
		{
			if (selectedNode.Tag is PrinterInfo printerInfo)
			{
				nextButton.Enabled = true;
			}

			base.OnTreeNodeSelected(selectedNode);
		}

		protected override void OnContinue(TreeNode treeNode)
		{
			if (treeNode.Tag is PrinterInfo printerInfo)
			{
				ProfileManager.LoadSettingsAsync(printerInfo.ID).ContinueWith(task =>
				{
					var settings = task.Result;
					printerLoaded?.Invoke(new PrinterConfig(settings));
				});
			}

			base.OnContinue(treeNode);
		}
	}
}