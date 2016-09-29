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

    using AdMaiora.Chatty.Api.Models;
    using AdMaiora.Chatty.Api.DataObjects;    

    using Microsoft.Azure.Mobile.Server;
    using Microsoft.Azure.Mobile.Server.Config;
    using Microsoft.Azure.NotificationHubs;

    using SendGrid;
    using SendGrid.Helpers.Mail;    

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

        public const string MAIL_SENDGRID_APIKEY = "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";

        private NotificationHubClient _nhclient;

        #endregion

        #region Constructors

        public UsersController()
        {            
            _nhclient = NotificationHubClient.CreateClientFromConnectionString(
                "Endpoint=sb://admaiora.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=Bee6TFs/KoDfbzMMtzUSJ7vJ00Pk7/uPOX4qpK80AKs=", "Chatty");
        }

        #endregion

        #region Users Endpoint Methods

        [HttpPost, Route("users/register")]
        public async Task<IHttpActionResult> RegisterUser([FromBody] Poco.User credentials)
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

                    SendGridAPIClient mc = new SendGridAPIClient(UsersController.MAIL_SENDGRID_APIKEY);

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
        public async Task<IHttpActionResult> VerifyUser([FromBody] Poco.User credentials)
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

                    SendGridAPIClient mc = new SendGridAPIClient(UsersController.MAIL_SENDGRID_APIKEY);

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
                    responseMsg.Headers.Location = new Uri("http://www.admaiorastudio.com/chatty/chatty.php");
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
        public IHttpActionResult LoginUser([FromBody] Poco.User credentials)
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
                    user.AuthAccessToken = Guid.NewGuid().ToString();
                    user.AuthExpirationDate = DateTime.Now.AddDays(UsersController.AUTH_TOKEN_MAX_DURATION);
                    ctx.SaveChanges();

                    _nhclient.SendGcmNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.Android.Make(
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

        [HttpPost, Route("users/logout")]
        public IHttpActionResult LogoutUser(string email)
        {
            if (!UsersController.IsAuthorized(this.Request))
                return Unauthorized();

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

        [HttpGet, Route("users/active")]
        public IHttpActionResult GetActiveUsers()
        {
            if (!UsersController.IsAuthorized(this.Request))
                return Unauthorized();

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

        public static bool IsAuthorized(HttpRequestMessage request)
        {
            string authAccessToken = request.GetHeaderOrDefault("Authorization");
            if (String.IsNullOrWhiteSpace(authAccessToken))
                return false;

            using (var ctx = new ChattyDbContext())
            {
                User user = ctx.Users.SingleOrDefault(x => x.AuthAccessToken == authAccessToken);
                if (user == null)
                    return false;

                if (DateTime.Now > user.AuthExpirationDate)
                    return false;

                return true;
            }
        }

        public static string GetRequestAuthAccessToken(HttpRequestMessage request)
        {
            string authAccessToken = request.GetHeaderOrDefault("Authorization");
            return authAccessToken;
        }

        #endregion
    }
}