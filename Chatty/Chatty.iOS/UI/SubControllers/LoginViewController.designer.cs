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
	[Register ("LoginViewController")]
	partial class LoginViewController
	{
		[Outlet]
		UIKit.UITextField EmailText { get; set; }

		[Outlet]
		UIKit.UIView InputLayout { get; set; }

		[Outlet]
		UIKit.UIButton LoginButton { get; set; }

		[Outlet]
		UIKit.UIView LoginLayout { get; set; }

		[Outlet]
		UIKit.UIImageView LogoImage { get; set; }

		[Outlet]
		UIKit.UITextField PasswordText { get; set; }

		[Outlet]
		UIKit.UIButton RegisterButton { get; set; }

		[Outlet]
		UIKit.UIButton VerifyButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (LoginLayout != null) {
				LoginLayout.Dispose ();
				LoginLayout = null;
			}

			if (LogoImage != null) {
				LogoImage.Dispose ();
				LogoImage = null;
			}

			if (InputLayout != null) {
				InputLayout.Dispose ();
				InputLayout = null;
			}

			if (EmailText != null) {
				EmailText.Dispose ();
				EmailText = null;
			}

			if (PasswordText != null) {
				PasswordText.Dispose ();
				PasswordText = null;
			}

			if (LoginButton != null) {
				LoginButton.Dispose ();
				LoginButton = null;
			}

			if (RegisterButton != null) {
				RegisterButton.Dispose ();
				RegisterButton = null;
			}

			if (VerifyButton != null) {
				VerifyButton.Dispose ();
				VerifyButton = null;
			}
		}
	}
}
