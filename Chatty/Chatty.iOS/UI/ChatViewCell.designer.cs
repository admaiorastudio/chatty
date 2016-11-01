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
	[Register ("ChatViewCell")]
	partial class ChatViewCell
	{
		[Outlet]
		public UIKit.UIView CalloutLayout { get; set; }

		[Outlet]
		public UIKit.UIView ContentLayout { get; set; }

		[Outlet]
		public UIKit.UILabel DateLabel { get; set; }

		[Outlet]
		public UIKit.UILabel MessageLabel { get; set; }

		[Outlet]
		public UIKit.UILabel SenderLabel { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (CalloutLayout != null) {
				CalloutLayout.Dispose ();
				CalloutLayout = null;
			}

			if (ContentLayout != null) {
				ContentLayout.Dispose ();
				ContentLayout = null;
			}

			if (SenderLabel != null) {
				SenderLabel.Dispose ();
				SenderLabel = null;
			}

			if (MessageLabel != null) {
				MessageLabel.Dispose ();
				MessageLabel = null;
			}

			if (DateLabel != null) {
				DateLabel.Dispose ();
				DateLabel = null;
			}
		}
	}
}
