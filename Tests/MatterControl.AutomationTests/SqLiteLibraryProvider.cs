﻿using System.Threading;
using System.Threading.Tasks;
using MatterHackers.MatterControl.PartPreviewWindow;
using NUnit.Framework;

namespace MatterHackers.MatterControl.Tests.Automation
{
	[TestFixture, Category("MatterControl.UI.Automation"), RunInApplicationDomain, Apartment(ApartmentState.STA)]
	public class SqLiteLibraryProviderTests
	{
		[Test]
		public async Task LibraryQueueViewRefreshesOnAddItem()
		{
			await MatterControlUtilities.RunTest((testRunner) =>
			{
				testRunner.OpenEmptyPartTab();

				testRunner.AddItemToBedplate();

				var view3D = testRunner.GetWidgetByName("View3DWidget", out _) as View3DWidget;
				var scene = view3D.InteractionLayer.Scene;

				testRunner.WaitFor(() => scene.SelectedItem != null);
				Assert.IsNotNull(scene.SelectedItem, "Expect part selection after Add to Bed action");

				testRunner.ClickByName("Duplicate Button");

				// wait for the copy to finish
				testRunner.Delay(.1);
				testRunner.ClickByName("Remove Button");

				testRunner.SaveBedplateToFolder("0Test Part", "Local Library Row Item Collection");

				// Click Home -> Local Library
				testRunner.NavigateToLibraryHome();
				testRunner.NavigateToFolder("Local Library Row Item Collection");

				// ensure that it is now in the library folder (that the folder updated)
				Assert.IsTrue(testRunner.WaitForName("Row Item 0Test Part"), "The part we added should be in the library");

				testRunner.Delay(.5);

				return Task.CompletedTask;
			}, queueItemFolderToAdd: QueueTemplate.Three_Queue_Items, overrideWidth: 1300);
		}
	}
}