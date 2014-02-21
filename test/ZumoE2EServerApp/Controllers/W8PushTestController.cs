// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Microsoft.ServiceBus.Notifications;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Notifications;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using System.Threading.Tasks;
using System.Web.Http;
using ZumoE2EServerApp.DataObjects;

namespace ZumoE2EServerApp.Controllers
{
    public class W8PushTestController : TableController<W8PushTestEntity>
    {
        [RequiresAuthorization(AuthorizationLevel.Anonymous)]
        public async Task<W8PushTestEntity> PostW8PushTestEntity(W8PushTestEntity item)
        {
            IPushMessage message = null;
            string tag = null;
            if (item.NHNotificationType == "template")
            {
                var tpmessage = new TemplatePushMessage();
                tpmessage.Add("News_English", "World News in English!");
                tpmessage.Add("News_French", "World News in French!");
                tpmessage.Add("News_Mandarin", "World News in Mandarin!");
                tpmessage.Add("News_Badge", "10");
                message = tpmessage;
                tag = "World";
            }
            else
            {
                var wpmessage = new WindowsPushMessage();
                if (item.NHNotificationType == "raw")
                {
                    wpmessage.XmlPayload = item.Payload;
                }
                else
                {
                    wpmessage.XmlPayload = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + item.XmlPayload;
                }

                wpmessage.Headers.Add("X-WNS-Type", "wns/" + item.NHNotificationType);
                message = wpmessage;
                tag = "tag1";
            }

            NotificationOutcome pushResponse = await this.Services.Push.SendAsync(message, tag);
            this.Services.Log.Info("WNS push sent: " + pushResponse, this.Request);
            return new W8PushTestEntity()
            {
                Id = "1",
                PushResponse = pushResponse.State.ToString() + "-" + pushResponse.TrackingId,
            };
        }
    }
}