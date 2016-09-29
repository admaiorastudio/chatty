namespace AdMaiora.Chatty.Api
{
    using System;
    
    public static class Poco
    {
        public class User
        {
            public int UserId;
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