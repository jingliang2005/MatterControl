﻿/*
Copyright (c) 2014, Kevin Pope
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.DataConverters3D;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.DataStorage;
using MatterHackers.MatterControl.Library;
using MatterHackers.MatterControl.SlicerConfiguration;

namespace MatterHackers.MatterControl.PrintQueue
{
	public static class PrintItemWrapperExtensionMethods
	{
		private static TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

		public static string GetFriendlyName(this PrintItemWrapper printItemWrapper)
		{
			if (printItemWrapper?.Name == null)
			{
				return "";
			}

			return textInfo?.ToTitleCase(printItemWrapper.Name.Replace('_', ' '));
		}

		public static string GetFriendlyName(string fileName)
		{
			if (fileName == null)
			{
				return "";
			}

			return textInfo?.ToTitleCase(fileName.Replace('_', ' '));
		}
	}

	public class PrintItemWrapper
	{
		private string fileType;
		PrinterConfig printer;

		public PrintItemWrapper(PrintItem printItem, ILibraryContainer sourceLibraryProviderLocator = null)
		{
			this.printer = printer;
			this.PrintItem = printItem;

			if (FileLocation != null)
			{
				this.fileType = Path.GetExtension(FileLocation).ToUpper();
			}

			SourceLibraryProviderLocator = sourceLibraryProviderLocator;
		}

		public PrintItemWrapper(int printItemId)
		{
			this.printer = printer;
			this.PrintItem = Datastore.Instance.dbSQLite.Table<PrintItem>().Where(v => v.Id == printItemId).Take(1).FirstOrDefault();
			try
			{
				this.fileType = Path.GetExtension(this.FileLocation).ToUpper();
			}
			catch(Exception e)
			{
				Debug.Print(e.Message);
				GuiWidget.BreakInDebugger();
				//file not found
			}
		}

		public string FileHashCode
		{
			get
			{
				if (File.Exists(this.FileLocation))
				{
					return HashGenerator.ComputeFileSHA1(this.FileLocation);
				}

				return "file-missing";
			}
		}

		public string FileLocation
		{
			get  => this.PrintItem.FileLocation;
			set => this.PrintItem.FileLocation = value;
		}

		public string Name
		{
			get => this.PrintItem.Name;
			set => this.PrintItem.Name = value;
		}

		public PrintItem PrintItem { get; set; }

		public bool SlicingHadError { get; private set; } = false;

		public ILibraryContainer SourceLibraryProviderLocator { get; private set; }

		public bool UseIncrementedNameDuringTypeChange { get; internal set; }

		public void Delete()
		{
			PrintItem.Delete();

			// Reset the Id field after calling delete to clear the association and ensure that future db operations
			// result in inserts rather than update statements on a missing row
			this.PrintItem.Id = 0;
		}
	}
}