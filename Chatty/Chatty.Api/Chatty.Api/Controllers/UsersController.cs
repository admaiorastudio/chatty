namespace AdMaiora.Chatty.Api.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Mail;
    using System.Web.Http;
    using System.Web.Http.Tracing;
    using System.Web.Security;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Linq;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Configuration;
    using System.Security.Claims;

    using AdMaiora.Chatty.Api.Models;
    using AdMaiora.Chatty.Api.DataObjects;

    using RestSharp;

    using Microsoft.Azure.Mobile.Server;
    using Microsoft.Azure.Mobile.Server.Config;
    using Microsoft.Azure.Mobile.Server.Login;
    using Microsoft.Azure.NotificationHubs;

    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System.IdentityModel.Tokens;

    // Use the MobileAppController attribute for each ApiController you want to use  
    // from your mobile clients     
    public class UsersController : ApiController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        // Authorization token duration (in days)
        public const int AUTH_TOKEN_MAX_DURATION = 1;

        // Numbers of logged user limit 
        public const int USERS_MAX_LOGGED = 50;
        // Interval to consider an user active (in minutes)
        public const int USERS_MAX_INACTIVE_TIME = 30;        

        private NotificationHubClient _nhclient;

        #endregion

        #region Constructors

        public UsersController()
        {                        
            _nhclient = NotificationHubClient.CreateClientFromConnectionString(
                ConfigurationManager.ConnectionStrings["MS_NotificationHubConnectionString"].ConnectionString, "Chatty");
        }

        #endregion

        #region Users Endpoint Methods

        [HttpPost, Route("users/register")]
        public async Task<IHttpActionResult> RegisterUser(Poco.User credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.Email))
                return BadRequest("The email is not valid!");

            if (string.IsNullOrWhiteSpace(credentials.Password))
                return BadRequest("The password is not valid!");

            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    User user = ctx.Users.SingleOrDefault(x => x.Email == credentials.Email);
                    if (user != null)
                        return InternalServerError(new InvalidOperationException("This email has already taken!"));

                    user = new User { Email = credentials.Email, Password = credentials.Password };
                    user.Ticket = Guid.NewGuid().ToString();
                    ctx.Users.Add(user);
                    ctx.SaveChanges();

                    string apiKey = System.Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                    SendGridAPIClient mc = new SendGridAPIClient(apiKey);

                    Email to = new Email(user.Email);
                    Email from = new Email("info@admaiorastudio.com");
                    string subject = "Welocme to Chatty!";
                    Content content = new Content("text/plain",
                        String.Format("Hi {0},\n\nYou registration on Chatty is almost complete. Please click on this link to confirm your registration!\n\n{1}",
                        user.Email.Split('@')[0],
                        String.Format("http://chatty-api.azurewebsites.net/users/confirm?ticket={0}", user.Ticket)));
                    Mail mail = new Mail(from, subject, to, content);

                    dynamic response = await mc.client.mail.send.post(requestBody: mail.Get());

                    return Ok(Dto.Wrap(new Poco.User
                    {
                        UserId = user.UserId,
                        Email = user.Email,                        
                        AuthAccessToken = null,
                        AuthExpirationDate = null

                    }));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("users/verify")]
        public async Task<IHttpActionResult> VerifyUser(Poco.User credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.Email))
                return BadRequest("The email is not valid!");

            if (string.IsNullOrWhiteSpace(credentials.Password))
                return BadRequest("The password is not valid!");

            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    User user = ctx.Users.SingleOrDefault(x => x.Email == credentials.Email);
                    if (user == null)
                        return InternalServerError(new InvalidOperationException("This email is not registered!"));

                    if (user.IsConfirmed)
                        return InternalServerError(new InvalidOperationException("This email has been already confirmed!"));

                    string p1 = FormsAuthentication.HashPasswordForStoringInConfigFile(user.Password, "MD5");
                    string p2 = FormsAuthentication.HashPasswordForStoringInConfigFile(credentials.Password, "MD5");
                    if (p1 != p2)
                        return InternalServerError(new InvalidOperationException("Your credentials seem to be not valid!"));

                    string apiKey = System.Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                    SendGridAPIClient mc = new SendGridAPIClient(apiKey);

                    Email to = new Email(user.Email);
                    Email from = new Email("info@admaiorastudio.com");
                    string subject = "Welocme to Chatty!";
                    Content content = new Content("text/plain",
                        String.Format("Hi {0},\n\nYou registration on Chatty is almost complete. Please click on this link to confirm your registration!\n\n{1}",
                        user.Email.Split('@')[0],
                        String.Format("http://chatty-api.azurewebsites.net/users/confirm?ticket={0}", user.Ticket)));
                    Mail mail = new Mail(from, subject, to, content);
                 
                    dynamic response = await mc.client.mail.send.post(requestBody: mail.Get());
                    if(response.StatusCode != System.Net.HttpStatusCode.Accepted)
                        return InternalServerError(new InvalidOperationException("Internal mail error. Retry later!"));

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet, Route("users/confirm")]
        public IHttpActionResult ConfirmUser(string ticket)
        {
            if (string.IsNullOrWhiteSpace(ticket))
                return BadRequest("The ticket is not valid!");

            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    User user = ctx.Users.SingleOrDefault(x => x.Ticket == ticket);
                    if (user == null)
                        return BadRequest("This ticket is not a real!");

                    user.IsConfirmed = true;
                    ctx.SaveChanges();

                    IHttpActionResult response;
                    //we want a 303 with the ability to set location
                    HttpResponseMessage responseMsg = new HttpResponseMessage(HttpStatusCode.RedirectMethod);
                    responseMsg.Headers.Location = new Uri("http://www.admaiorastudio.com/chatty.php");
                    response = ResponseMessage(responseMsg);
                    return response;
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("users/login")]
        public IHttpActionResult LoginUser(Poco.User credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.Email))
                return BadRequest("The email is not valid!");

            if (string.IsNullOrWhiteSpace(credentials.Password))
                return BadRequest("The password is not valid!");
            
            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    User user = ctx.Users.SingleOrDefault(x => x.Email == credentials.Email);
                    if (user == null)
                        return Unauthorized();

                    if (!user.IsConfirmed)
                        return InternalServerError(new InvalidOperationException("You must confirm your email first!"));

                    if (!String.IsNullOrWhiteSpace(user.FacebookId) && user.Password == null)
                        return InternalServerError(new InvalidOperationException("You must login via Facebook!"));

                    string p1 = FormsAuthentication.HashPasswordForStoringInConfigFile(user.Password, "MD5");
                    string p2 = FormsAuthentication.HashPasswordForStoringInConfigFile(credentials.Password, "MD5");
                    if (p1 != p2)
                        return Unauthorized();                    

                    int activeUsers =
                        ctx.Users.Count(x => x.LastActiveDate.HasValue 
                            && DbFunctions.DiffDays(DateTime.Now, x.AuthExpirationDate.Value) < UsersController.AUTH_TOKEN_MAX_DURATION);

                    if (activeUsers == USERS_MAX_LOGGED)
                    {
                        // Check if we can kick out a user marked as not active
                        User userToKick = ctx.Users
                            .Where(x => x.LastActiveDate.HasValue)
                            .Where(x => DbFunctions.DiffMinutes(DateTime.Now, x.LastActiveDate.Value) >= USERS_MAX_INACTIVE_TIME)
                            .OrderBy(x => x.LastActiveDate.GetValueOrDefault())
                            .SingleOrDefault();

                        // We got a candidate?
                        if (userToKick != null)
                        {
                            userToKick.LoginDate = null;
                            userToKick.LastActiveDate = null;
                            userToKick.AuthAccessToken = null;
                            userToKick.AuthExpirationDate = null;
                        }
                        else
                        {
                            return InternalServerError(new InvalidOperationException("Max user logged reached. Please retry later!"));
                        }
                    }

                    user.LoginDate = DateTime.Now.ToUniversalTime();
                    user.LastActiveDate = user.LoginDate;
                    user.AuthAccessToken = GetAuthenticationTokenForUser(user.Email).RawData;
                    user.AuthExpirationDate = DateTime.Now.AddDays(UsersController.AUTH_TOKEN_MAX_DURATION);
                    ctx.SaveChanges();

                    _nhclient.SendGcmNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.Android.Make(
                            "New user connected",
                            String.Format("User {0} has joined the chat.", credentials.Email.Split('@')[0]),
                            2,
                            credentials.Email.Split('@')[0]
                            )), String.Concat("!", user.Email));

                    _nhclient.SendAppleNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.iOS.Make(
                            "New user connected",
                            String.Format("User {0} has joined the chat.", credentials.Email.Split('@')[0]),
                            2,
                            credentials.Email.Split('@')[0]
                            )), String.Concat("!", user.Email));

                    return Ok(Dto.Wrap(new Poco.User
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        LoginDate = user.LoginDate,
                        AuthAccessToken = user.AuthAccessToken,
                        AuthExpirationDate = user.AuthExpirationDate
                    }));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route("users/login/fb")]
        public async Task<IHttpActionResult> LoginUser(Facebook.Credentials credentials)
        {
            if (string.IsNullOrWhiteSpace(credentials.UserId))
                return BadRequest("The Facebook User ID is not valid!");

            if (string.IsNullOrWhiteSpace(credentials.Email))
                return BadRequest("The email is not valid!");

            if (string.IsNullOrWhiteSpace(credentials.Token))
                return BadRequest("The Facebook token is not valid!");

            try
            {
                RestClient c = new RestClient(new Uri("https://graph.facebook.com/"));

                // To login via facebook token, we need first to validate the token passed
                // To validate the token we must check if it belongs to our FB application
                // Reference: http://stackoverflow.com/questions/5406859/facebook-access-token-server-side-validation-for-iphone-app

                // Access token request
                RestRequest tr = new RestRequest("oauth/access_token", Method.GET);
                tr.AddParameter("client_id", ConfigurationManager.AppSettings["FB_APP_ID"]);
                tr.AddParameter("client_secret", ConfigurationManager.AppSettings["FB_APP_SECRET"]);
                tr.AddParameter("grant_type", "client_credentials");
                var r1 = await c.ExecuteTaskAsync(tr);

                if (r1.StatusCode != HttpStatusCode.OK)
                    return InternalServerError(new InvalidOperationException("Unable to login via Facebook"));

                if (String.IsNullOrWhiteSpace(r1.Content)
                    || !r1.Content.Contains("access_token="))
                {
                    return InternalServerError(new InvalidOperationException("Unable to login via Facebook"));
                }

                string accessToken = r1.Content.Split('=')[1];

                // Validation request
                RestRequest vr = new RestRequest("debug_token", Method.GET);
                vr.AddParameter("input_token", credentials.Token);
                vr.AddParameter("access_token", accessToken);                
                var r2 = await c.ExecuteTaskAsync<Facebook.DebugToken>(vr);
                if (r2.StatusCode != HttpStatusCode.OK)
                    return InternalServerError(new InvalidOperationException("Unable to login via Facebook"));
                
                if(r2.Data.data.app_id != ConfigurationManager.AppSettings["FB_APP_ID"]                    
                    || r2.Data.data.user_id != credentials.UserId
                    || !r2.Data.data.is_valid)
                {
                    return InternalServerError(new InvalidOperationException("Unable to login via Facebook"));
                }

                using (var ctx = new ChattyDbContext())
                {
                    // Check if we have already registered the user, if not this login method will take care of it
                    User user = ctx.Users.SingleOrDefault(x => x.Email == credentials.Email);
                    if (user == null)
                    {
                        user = new User
                        {
                            FacebookId = credentials.UserId,
                            Email = credentials.Email,
                            Password = null,
                            Ticket = Guid.NewGuid().ToString(),
                            IsConfirmed = true
                        };

                        ctx.Users.Add(user);
                        ctx.SaveChanges();
                    }
                    else
                    {
                        user.FacebookId = credentials.UserId;                        
                        user.IsConfirmed = true;

                        ctx.SaveChanges();
                    }

                    int activeUsers =
                        ctx.Users.Count(x => x.LastActiveDate.HasValue
                            && DbFunctions.DiffDays(DateTime.Now, x.AuthExpirationDate.Value) < UsersController.AUTH_TOKEN_MAX_DURATION);

                    if (activeUsers == USERS_MAX_LOGGED)
                    {
                        // Check if we can kick out a user marked as not active
                        User userToKick = ctx.Users
                            .Where(x => x.LastActiveDate.HasValue)
                            .Where(x => DbFunctions.DiffMinutes(DateTime.Now, x.LastActiveDate.Value) >= USERS_MAX_INACTIVE_TIME)
                            .OrderBy(x => x.LastActiveDate.GetValueOrDefault())
                            .SingleOrDefault();

                        // We got a candidate?
                        if (userToKick != null)
                        {
                            userToKick.LoginDate = null;
                            userToKick.LastActiveDate = null;
                            userToKick.AuthAccessToken = null;
                            userToKick.AuthExpirationDate = null;
                        }
                        else
                        {
                            return InternalServerError(new InvalidOperationException("Max user logged reached. Please retry later!"));
                        }
                    }

                    user.LoginDate = DateTime.Now.ToUniversalTime();
                    user.LastActiveDate = user.LoginDate;
                    user.AuthAccessToken = user.AuthAccessToken = GetAuthenticationTokenForUser(user.Email).RawData;
                    user.AuthExpirationDate = DateTime.Now.AddDays(UsersController.AUTH_TOKEN_MAX_DURATION);
                    ctx.SaveChanges();

                    await _nhclient.SendGcmNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.Android.Make(
                            "New user connected",
                            String.Format("User {0} has joined the chat.", credentials.Email.Split('@')[0]),
                            2,
                            credentials.Email.Split('@')[0]
                            )), String.Concat("!", user.Email));

                    await _nhclient.SendAppleNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.iOS.Make(
                            "New user connected",
                            String.Format("User {0} has joined the chat.", credentials.Email.Split('@')[0]),
                            2,
                            credentials.Email.Split('@')[0]
                            )), String.Concat("!", user.Email));

                    return Ok(Dto.Wrap(new Poco.User
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        LoginDate = user.LoginDate,
                        AuthAccessToken = user.AuthAccessToken,
                        AuthExpirationDate = user.AuthExpirationDate
                    }));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet, Route("users/restore")]
        public IHttpActionResult RestoreUser(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
                return BadRequest("The access token is not valid!");

            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    User user = ctx.Users.SingleOrDefault(x => x.AuthAccessToken == accessToken);
                    if (user == null)
                        return Unauthorized();

                    int activeUsers =
                        ctx.Users.Count(x => x.LastActiveDate.HasValue
                            && DbFunctions.DiffDays(DateTime.Now, x.AuthExpirationDate.Value) < UsersController.AUTH_TOKEN_MAX_DURATION);

                    if (activeUsers == USERS_MAX_LOGGED)
                    {
                        // Check if we can kick out a user marked as not active
                        User userToKick = ctx.Users
                            .Where(x => x.LastActiveDate.HasValue)
                            .Where(x => DbFunctions.DiffMinutes(DateTime.Now, x.LastActiveDate.Value) >= USERS_MAX_INACTIVE_TIME)
                            .OrderBy(x => x.LastActiveDate.GetValueOrDefault())
                            .SingleOrDefault();

                        // We got a candidate?
                        if (userToKick != null)
                        {
                            userToKick.LoginDate = null;
                            userToKick.LastActiveDate = null;
                            userToKick.AuthAccessToken = null;
                            userToKick.AuthExpirationDate = null;
                        }
                        else
                        {
                            return InternalServerError(new InvalidOperationException("Max user logged reached. Please retry later!"));
                        }
                    }

                    user.LoginDate = DateTime.Now.ToUniversalTime();
                    user.LastActiveDate = user.LoginDate;
                    ctx.SaveChanges();

                    _nhclient.SendGcmNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.Android.Make(
                            "New user connected",
                            String.Format("User {0} has joined the chat.", user.Email.Split('@')[0]),
                            2,
                            user.Email.Split('@')[0]
                            )), String.Concat("!", user.Email));

                    _nhclient.SendAppleNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.iOS.Make(
                            "New user connected",
                            String.Format("User {0} has joined the chat.", user.Email.Split('@')[0]),
                            2,
                            user.Email.Split('@')[0]
                            )), String.Concat("!", user.Email));

                    return Ok(Dto.Wrap(new Poco.User
                    {
                        UserId = user.UserId,
                        Email = user.Email,
                        LoginDate = user.LoginDate,
                        AuthAccessToken = user.AuthAccessToken,
                        AuthExpirationDate = user.AuthExpirationDate
                    }));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [HttpPost, Route("users/logout")]
        public IHttpActionResult LogoutUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("The email is not valid!");

            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    User user = ctx.Users.SingleOrDefault(x => x.Email == email);
                    if (user == null)
                        return Unauthorized();

                    user.LoginDate = null;
                    user.LastActiveDate = null;
                    user.AuthAccessToken = null;
                    user.AuthExpirationDate = null;
                    ctx.SaveChanges();

                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [HttpGet, Route("users/active")]
        public IHttpActionResult GetActiveUsers()
        {
            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    return Ok(ctx.Users
                    .Where(x => (DateTime.Now - x.LastActiveDate.GetValueOrDefault(DateTime.MinValue)).Minutes < UsersController.USERS_MAX_INACTIVE_TIME)
                    .Select(x => new Poco.User
                    {
                        UserId = x.UserId,
                        Email = x.Email
                    })
                    .ToList());
                }
            }
            catch(Exception ex)
            {
                return InternalServerError(ex);

            }
        }

        #endregion

        #region Methods

        private JwtSecurityToken GetAuthenticationTokenForUser(string email)
        {
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email.Split('@')[0]),
                new Claim(JwtRegisteredClaimNames.Email, email),
            };

            var signingKey = Environment.GetEnvironmentVariable("WEBSITE_AUTH_SIGNING_KEY");
            var audience = "https://chatty-api.azurewebsites.net/";
            var issuer = "https://chatty-api.azurewebsites.net/";
            
            var token = AppServiceLoginHandler.CreateToken(
                claims,
                signingKey,
                audience,
                issuer,
                TimeSpan.FromHours(24)
                );

            return token;
        }

        //public static bool IsAuthorized(HttpRequestMessage request)
        //{
        //    string authAccessToken = request.GetHeaderOrDefault("Authorization");
        //    if (String.IsNullOrWhiteSpace(authAccessToken))
        //        return false;

        //    using (var ctx = new ChattyDbContext())
        //    {
        //        User user = ctx.Users.SingleOrDefault(x => x.AuthAccessToken == authAccessToken);
        //        if (user == null)
        //            return false;

        //        if (DateTime.Now > user.AuthExpirationDate)
        //            return false;

        //        return true;
        //    }
        //}

        public static string GetRequestAuthAccessToken(HttpRequestMessage request)
        {
            string authAccessToken = request.GetHeaderOrDefault("Authorization");
            return authAccessToken;
        }

        #endregion
    }
}