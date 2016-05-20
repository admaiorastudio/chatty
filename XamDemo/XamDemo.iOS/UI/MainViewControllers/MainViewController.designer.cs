// WARNING
//
// This file has been generated automatically by Xamarin Studio Business to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace AdMaiora.XamDemo
{
	[Register ("MainViewController")]
	partial class MainViewController
	{
		[Outlet]
		UIKit.UIView ContentLayout { get; set; }

		[Outlet]
		UIKit.UIView LoadLayout { get; set; }

		[Outlet]
		UIKit.UIActivityIndicatorView LoadProgress { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (ContentLayout != null) {
				ContentLayout.Dispose ();
				ContentLayout = null;
			}

			if (LoadLayout != null) {
				LoadLayout.Dispose ();
				LoadLayout = null;
			}

			if (LoadProgress != null) {
				LoadProgress.Dispose ();
				LoadProgress = null;
			}
		}
	}
}
