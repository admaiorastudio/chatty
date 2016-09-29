namespace AdMaiora.Chatty.Api
{
    using System;

    public static class Push
    {
        public class Android
        {
            #region Definition

            public class Data
            {
                public string title;
                public string body;
                public int action;
                public string payload;
            }

            public Data data;

            #endregion

            public static Push.Android Make(string title, string body, int action, string payload)
            {
                return new Push.Android
                {
                    data = new Data
                    {
                        title = title,
                        body = body,
                        action = action,
                        payload = payload
                    }
                };
            }
        }

        public class iOS
        {
            #region Definition

            public class Aps
            {
                public class Alert
                {
                    public string title;
                    public string body;
                }

                public Alert alert;
            }

            public class Data
            {
                public int action;
                public string payload;
            }

            public Aps aps;
            public Data data;

            #endregion

            public static Push.iOS Make(string title, string body, int action, string payload)
            {
                return new Push.iOS
                {
                    aps = new Aps
                    {
                        alert = new Aps.Alert
                        {
                            title = title,
                            body = body
                        }
                    },

                    data = new Data
                    {
                        action = action,
                        payload = payload
                    }
                };
            }
        }
    }
}