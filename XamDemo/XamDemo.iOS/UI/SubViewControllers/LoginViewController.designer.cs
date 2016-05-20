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
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		UIKit.UITextField PasswordText { get; set; }

		[Outlet]
		UIKit.UITextField UsernameText { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (UsernameText != null) {
				UsernameText.Dispose ();
				UsernameText = null;
			}

			if (PasswordText != null) {
				PasswordText.Dispose ();
				PasswordText = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}
		}
	}
}
