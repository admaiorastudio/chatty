using System;

using Foundation;
using UIKit;

namespace AdMaiora.Chatty
{
    public partial class ChatViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString ("ChatViewCell");
        public static readonly UINib Nib;

        static ChatViewCell ()
        {
            Nib = UINib.FromName ("ChatViewCell", NSBundle.MainBundle);
        }

        protected ChatViewCell (IntPtr handle) : base (handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
    }
}
