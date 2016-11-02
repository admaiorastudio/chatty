namespace AdMaiora.Chatty.Api
{
    using System;

    public static class Poco
    {
        public class User
        {
            public int UserId;
            public string FacebookId;
            public string Email;
            public string Password;
            public DateTime? LoginDate;
            public string AuthAccessToken;
            public DateTime? AuthExpirationDate;
        }
        
        public class Message
        {
            public int MessageId;
            public string Content;
            public string Sender;
            public DateTime? SendDate;
        }

        public class Bulk
        {
            public Message[] Messages;
        }
    }

    public static class Facebook
    {
        public class Credentials
        {
            public string UserId;            
            public string Email;
            public string Token;
        }

        public class DebugToken
        {
            public class Data
            {
                public string app_id { get; set; }
                public bool is_valid { get; set; }
                //public Metadata metadata { get; set; }
                public string application { get; set; }
                public string user_id { get; set; }
                public int issued_at { get; set; }
                public int expires_at { get; set; }
                //public object[] scopes { get; set; }
            }

            public class Metadata
            {
                public string sso { get; set; }
            }

            public Data data { get; set; }
        }
    }

    public static class Dto
    {
        public class Response
        {
            public string Message;

            public string ExceptionMessage;
            public string ExceptionType;
        }

        public class Response<TContent> : Response
        {
            public TContent Content;

            public Response(TContent content)
            {
                this.Content = content;
            }
        }

        public static Response<T> Wrap<T>(T content)
        {
            return new Response<T>(content);
        }
    }
}