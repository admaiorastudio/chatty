using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using RestSharp.Portable;
using RestSharp.Portable.HttpClient;

namespace AdMaiora.XamDemo
{
    public static class DTO
    {
        public class BaseResponse
        {
            public string status;
            public string message;
        }

        public class UserLogin : BaseResponse
        {
            public string username;
            public string token;
        }

        public class UserLogout : BaseResponse
        {

        }

        public class UserList : BaseResponse
        {
            public class User
            {
                public string username;
            }

            public List<User> users;
        }

        public class MessageList : BaseResponse
        {
            public class Message
            {
                public string username;
                public string content;
            }

            public List<Message> messages;
        }
    }

    public class RestManager
    {
        #region Constants and Fields

        // Server resources location
        public const string ResourcesUrl = "http://xamdemo.getsandbox.com";
        // Default client timeout in seconds
        public const int RequestTimeout = 5;
        // Default client refresh interval in seconds
        public const int RefreshInterval = 5;
        
        // Current user name
        public static string CurrentUser = null;
        // Access token for protected endpoints
        public static string AccessToken = null;

        #endregion

        #region Public Methods

        public async Task LoginUser(CancellationTokenSource cts, string username, string password, Action<DTO.UserLogin> success, Action<string> error, Action finished)
        {
            try
            {
                // Create the rest client
                var client = GetRestClient();
                // Create the rest request
                var request = GetRestRequest(
                    // HTTP method
                    Method.POST,
                    // Resource to call
                    "/users/login",
                    // Access token (not used for this endpoint)
                    null,
                    // Parameters as anonymous object. Will be jsonized
                    new
                    {
                        username = username,
                        password = password
                    });

                var response = await client.Execute<DTO.UserLogin>(request, cts.Token);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (success != null)
                        success(response.Data);
                }
                else
                {
                    if (error != null)
                        error(response.Data.message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                if (error != null)
                    error("Internal Error :(");
            }
            finally
            {
                if (finished != null)
                    finished();
            }
        }

        public async Task GetMessages(CancellationTokenSource cts, Action<DTO.MessageList> success, Action<string> error, Action finished)
        {
            try
            {
                // Create the rest client
                var client = GetRestClient();
                // Create the rest request
                var request = GetRestRequest(
                    // HTTP method
                    Method.GET,
                    // Resource to call
                    "/messages/all",
                    // Access token (not used for this endpoint)
                    RestManager.AccessToken,
                    // Parameters as anonymous object.
                    null);

                

                var response = await client.Execute<DTO.MessageList>(request, cts.Token);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (success != null)
                        success(response.Data);
                }
                else
                {
                    if (error != null)
                        error(response.Data.message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                if (error != null)
                    error("Internal Error :(");
            }
            finally
            {
                if (finished != null)
                    finished();
            }
        }

        public async Task SendMessage(CancellationTokenSource cts, string content, Action<DTO.MessageList> success, Action<string> error, Action finished)
        {
            try
            {
                // Create the rest client
                var client = GetRestClient();
                // Create the rest request
                var request = GetRestRequest(
                    // HTTP method
                    Method.POST,
                    // Resource to call
                    "/messages/send",
                    // Access token (not used for this endpoint)
                    RestManager.AccessToken,
                    // Parameters as anonymous object. Will be jsonized
                    new
                    {
                        username = RestManager.CurrentUser,
                        content =  content
                    });

                var response = await client.Execute<DTO.MessageList>(request, cts.Token);
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (success != null)
                        success(response.Data);
                }
                else
                {
                    if (error != null)
                        error(response.Data.message);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());

                if (error != null)
                    error("Internal Error :(");
            }
            finally
            {
                if (finished != null)
                    finished();
            }
        }


        #endregion

        #region Helper Methods

        public string CreateHTMLFromMessages(DTO.MessageList list)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<body style='background-color: #313131; color: #ffffff; font-family: helvetica;'>");

            if (list.messages == null
                || list.messages.Count == 0)
            {
                return "<p>There is no messages!</p>";
            }
            else
            {
                foreach (var message in list.messages)
                {
                    sb.AppendLine(String.Format("<p style='margin-top: 2px;'><b>{0}:</b>&nbsp;<i>{1}</i></p>", 
                        System.Net.WebUtility.HtmlEncode(message.username),
                        System.Net.WebUtility.HtmlEncode(message.content)));
                }

                sb.Append("<br id='bottom' />");
                sb.AppendLine("<script>document.getElementById('bottom').scrollIntoView();</script>");
            }

            sb.AppendLine("</body>");

            return sb.ToString();
        }

        public string CreateHTMLFromError(string error)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<body style='background-color: #313131; color: #ffffff;'>");
            sb.AppendLine("<p><b>Unable to get messages...</b></p>");
            sb.AppendLine(String.Format("<p><b>Error:</b>&nbsp;{0}</p>", error));
            sb.AppendLine("</body>");

            return sb.ToString();
        }

        #endregion

        #region Methods

        private RestClient GetRestClient()
        {
            RestClient client = new RestClient(RestManager.ResourcesUrl);
            client.Timeout = TimeSpan.FromSeconds(RestManager.RequestTimeout);

            return client;
        }

        private RestRequest GetRestRequest(Method method, string resource, string token, object parameters)
        {
            RestRequest request = new RestRequest(resource, method);            
            request.AddHeader("Content-Type", "application/json");

            if (!String.IsNullOrWhiteSpace(token))
                request.AddHeader("Access-Token", token);

            if (method == Method.POST)
            {
                if (parameters == null)
                    parameters = new { };

                request.AddBody(parameters);
            }

            // Needed because windows phone cache the requests
            request.AddParameter("noCache", DateTime.UtcNow.ToString());

            return request;
        }

        #endregion
    }
}
