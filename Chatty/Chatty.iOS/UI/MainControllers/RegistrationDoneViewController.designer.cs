// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace AdMaiora.Chatty
{
	[Register ("RegistrationDoneViewController")]
	partial class RegistrationDoneViewController
	{
		[Outlet]
		UIKit.UIImageView CatImage { get; set; }

		[Outlet]
		UIKit.UIButton GoToLoginButton { get; set; }

		[Outlet]
		UIKit.UILabel WelcomeLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CatImage != null) {
				CatImage.Dispose ();
				CatImage = null;
			}

			if (WelcomeLabel != null) {
				WelcomeLabel.Dispose ();
				WelcomeLabel = null;
			}

			if (GoToLoginButton != null) {
				GoToLoginButton.Dispose ();
				GoToLoginButton = null;
			}
		}
	}
}
