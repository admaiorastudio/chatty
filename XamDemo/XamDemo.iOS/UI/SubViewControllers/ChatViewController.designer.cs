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
	[Register ("ChatViewController")]
	partial class ChatViewController
	{
		[Outlet]
		UIKit.UIWebView ChatView { get; set; }

		[Outlet]
		UIKit.UITextField MessageText { get; set; }

		[Outlet]
		UIKit.UIButton SendButton { get; set; }
        
		void ReleaseDesignerOutlets ()
		{
			if (MessageText != null) {
				MessageText.Dispose ();
				MessageText = null;
			}

			if (ChatView != null) {
				ChatView.Dispose ();
				ChatView = null;
			}

			if (SendButton != null) {
				SendButton.Dispose ();
				SendButton = null;
			}
		}
	}
}
