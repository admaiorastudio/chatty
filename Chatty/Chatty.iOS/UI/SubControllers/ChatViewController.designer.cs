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
	[Register ("ChatViewController")]
	partial class ChatViewController
	{
		[Outlet]
		UIKit.UIView BackLayout { get; set; }

		[Outlet]
		UIKit.UIImageView CalloutImage { get; set; }

		[Outlet]
		UIKit.UIView InputLayout { get; set; }

		[Outlet]
		AdMaiora.AppKit.UI.UIItemListView MessageList { get; set; }

		[Outlet]
		UIKit.UITextView MessageText { get; set; }

		[Outlet]
		UIKit.UIButton SendButton { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (BackLayout != null) {
				BackLayout.Dispose ();
				BackLayout = null;
			}

			if (InputLayout != null) {
				InputLayout.Dispose ();
				InputLayout = null;
			}

			if (MessageList != null) {
				MessageList.Dispose ();
				MessageList = null;
			}

			if (MessageText != null) {
				MessageText.Dispose ();
				MessageText = null;
			}

			if (CalloutImage != null) {
				CalloutImage.Dispose ();
				CalloutImage = null;
			}

			if (SendButton != null) {
				SendButton.Dispose ();
				SendButton = null;
			}
		}
	}
}
