namespace AdMaiora.Chatty
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Foundation;
    using UIKit;

    using AdMaiora.AppKit.UI;
    using AdMaiora.AppKit.UI.App;

    public partial class ChatViewController : AdMaiora.AppKit.UI.App.UISubViewController, IBackButton
    {
        #region Inner Classes

        class Message
        {
            #region Properties

            public int MessageId
            {
                get;
                set;
            }

            public string Content
            {
                get;
                set;
            }

            public DateTime SendDate
            {
                get;
                set;
            }

            public string Sender
            {
                get;
                set;
            }

            #endregion
        }

        #endregion

        #region Constants and Fields

        private const string ReceiverLock = "ReceiverLock";

        private string _email;
        private string _username;

        private int _lastMessageId;

        //private ChatAdapter _adapter;

        // This cancellation token is used to cancel the UI blocking until connection is done
        private CancellationTokenSource _cts0;

        // This flag check if we are already calling the login REST service
        private bool _isSendingMessage;
        // This cancellation token is used to cancel the rest send message request
        private CancellationTokenSource _cts1;

        // This cancellation token is used to cancel the rest refresh messages request
        private CancellationTokenSource _cts2;

        #endregion

        #region Constructors

        public ChatViewController()
            : base("ChatViewController", null)
        {
        }

        #endregion

        #region Properties
        #endregion

        #region ViewController Methods

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            #region Designer Stuff

            ResizeToShowKeyboard();

            #endregion

            this.MessageText.Text = String.Empty;
            this.MessageText.Constraints.Single(x => x.GetIdentifier() == "Height").Constant = 30f;                       
            this.MessageText.Changed += MessageText_Changed;
        }

        private void MessageText_Changed(object sender, EventArgs e)
        {
            UITextView t = sender as UITextView;

            nfloat textWidth = t.TextContainerInset.InsetRect(t.Frame).Width;
            textWidth -= 2.0f * t.TextContainer.LineFragmentPadding;
            
            var size = (new NSString(t.Text)).GetBoundingRect(
                (new CoreGraphics.CGSize(textWidth, Double.MaxValue)),
                NSStringDrawingOptions.UsesLineFragmentOrigin,
                new UIStringAttributes() { Font = t.Font },
                null).Size;
            
            int numberOfLines = (int)Math.Round(size.Height / t.Font.LineHeight);

            if (numberOfLines > 0 && numberOfLines < 4)
                this.MessageText.Constraints.Single(x => x.GetIdentifier() == "Height").Constant = 30f + (numberOfLines - 1) * t.Font.LineHeight;

            if (numberOfLines == 2)
            {
                this.MessageText.SetContentOffset(CoreGraphics.CGPoint.Empty, true);
                this.MessageText.SetNeedsDisplay();
            }
        }

        public bool ViewWillPop()
        {
            return true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);
        }
        #endregion

        #region Public Methods
        #endregion

        #region Methods
        #endregion

        #region Event Handlers
        #endregion
    }
}


