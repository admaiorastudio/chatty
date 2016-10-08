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
            #endregion
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


