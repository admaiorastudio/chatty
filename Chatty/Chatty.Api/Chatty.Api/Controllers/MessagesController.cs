namespace AdMaiora.Chatty.Api.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;
    using System.Web.Http.Tracing;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Linq;
    using System.Configuration;

    using AdMaiora.Chatty.Api.Models;
    using AdMaiora.Chatty.Api.DataObjects;    

    using Microsoft.Azure.Mobile.Server;
    using Microsoft.Azure.Mobile.Server.Config;
    using Microsoft.Azure.NotificationHubs;

    // Use the MobileAppController attribute for each ApiController you want to use  
    // from your mobile clients     
    public class MessagesController : ApiController
    {
        #region Inner Classes
        #endregion

        #region Constants and Fields

        private NotificationHubClient _nhclient;

        #endregion

        #region Constructors

        public MessagesController()
        {            
            _nhclient = NotificationHubClient.CreateClientFromConnectionString(
                ConfigurationManager.ConnectionStrings["MS_NotificationHubConnectionString"].ConnectionString, "Chatty");
        }

        #endregion

        #region Messages Endpoint Methods

        [Authorize]
        [HttpPost, Route("messages/send")]
        public IHttpActionResult SendMessage(Poco.Message message)
        {
            //if (!UsersController.IsAuthorized(this.Request))
            //    return Unauthorized();

            if (string.IsNullOrWhiteSpace(message.Sender))
                return BadRequest("The sender is not valid!");

            if (string.IsNullOrWhiteSpace(message.Content))
                return BadRequest("The password is not valid!");

            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    string authAccessToken = UsersController.GetRequestAuthAccessToken(this.Request);
                    User user = ctx.Users.Single(x => x.AuthAccessToken == authAccessToken);
                    user.LastActiveDate = DateTime.Now.ToUniversalTime();

                    Message m = new Message { Content = message.Content, Sender = message.Sender, SendDate = DateTime.Now.ToUniversalTime() };
                    ctx.Messages.Add(m);

                    ctx.SaveChanges();

                    _nhclient.SendGcmNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.Android.Make(
                            "New messages",
                            "You have new unread messages!",
                            1,
                            m.MessageId.ToString()
                    )), String.Concat("!", user.Email));

                    _nhclient.SendAppleNativeNotificationAsync(
                        Newtonsoft.Json.JsonConvert.SerializeObject(Push.iOS.Make(
                            "New messages",
                            "You have new unread messages!",
                            1,
                            m.MessageId.ToString()
                    )), String.Concat("!", user.Email));

                    return Ok(Dto.Wrap(new Poco.Message
                    {
                        MessageId = m.MessageId,
                        Content = m.Content,
                        Sender = m.Sender,
                        SendDate = m.SendDate
                    }));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [HttpGet, Route("messages/new")]
        public IHttpActionResult GetNewMessages(int lastMessageId, string me)
        {
            //if (!UsersController.IsAuthorized(this.Request))
            //    return Unauthorized();

            if (lastMessageId == 0)
                return InternalServerError(new InvalidOperationException("Invalid message Id"));

            try
            {
                using (var ctx = new ChattyDbContext())
                {
                    string authAccessToken = UsersController.GetRequestAuthAccessToken(this.Request);
                    User user = ctx.Users.Single(x => x.AuthAccessToken == authAccessToken);
                    user.LastActiveDate = DateTime.Now.ToUniversalTime();
                    ctx.SaveChanges();

                    if (lastMessageId > ctx.Messages.OrderByDescending(x => x.MessageId).Take(1).ToList().FirstOrDefault()?.MessageId)
                        return InternalServerError(new InvalidOperationException("Invalid message Id"));

                    return Ok(Dto.Wrap(new Poco.Bulk
                    {
                        Messages = ctx.Messages
                            .Where(x => x.Sender != me && x.MessageId >= lastMessageId)
                            .Select(x => new Poco.Message
                            {
                                MessageId = x.MessageId,
                                Content = x.Content,
                                Sender = x.Sender,
                                SendDate = x.SendDate
                            })
                            .ToArray()
                    }));
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        #endregion
    }
}