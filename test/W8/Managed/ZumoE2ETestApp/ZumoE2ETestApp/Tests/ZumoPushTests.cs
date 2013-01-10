using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Data.Json;
using Windows.Networking.PushNotifications;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoPushTests
    {
        private static PushNotificationChannel pushChannel;
        private static Queue<PushNotificationReceivedEventArgs> pushesReceived = new Queue<PushNotificationReceivedEventArgs>();

        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Push tests");
            const string imageUrl = "http://zumotestserver.azurewebsites.net/content/zumo2.png";
            result.AddTest(CreateRegisterChannelTest());
            result.AddTest(CreateToastPushTest("sendToastText01", "hello world"));
            result.AddTest(CreateToastPushTest("sendToastImageAndText03", "hello", "world", null, imageUrl, "zumo"));
            result.AddTest(CreateToastPushTest("sendToastImageAndText04", "hello", "world", "how are you", imageUrl, "zumo"));
            result.AddTest(CreateBadgePushTest(4));
            result.AddTest(CreateBadgePushTest("playing"));
            result.AddTest(CreateUnregisterChannelTest());
            return result;
        }

        private static ZumoTest CreateToastPushTest(string wnsMethod, string text1, string text2 = null, string text3 = null, string imageUrl = null, string imageAlt = null)
        {
            JsonObject payload = new JsonObject();
            payload.Add("text1", JsonValue.CreateStringValue(text1));
            AddIfNotNull(payload, "text2", text2);
            AddIfNotNull(payload, "text3", text3);
            AddIfNotNull(payload, "image1src", imageUrl);
            AddIfNotNull(payload, "image1alt", imageAlt);
            XElement binding = new XElement("binding", new XAttribute("template", wnsMethod.Substring("send".Length)));
            if (imageUrl != null)
            {
                XElement image = new XElement("image",
                    new XAttribute("id", "1"),
                    new XAttribute("src", imageUrl),
                    new XAttribute("alt", imageAlt));
                binding.Add(image);
            }

            binding.Add(new XElement("text", new XAttribute("id", 1), new XText(text1)));
            if (text2 != null)
            {
                binding.Add(new XElement("text", new XAttribute("id", 2), new XText(text2)));
            }

            if (text3 != null)
            {
                binding.Add(new XElement("text", new XAttribute("id", 3), new XText(text2)));
            }

            XElement expectedResult = new XElement("toast",
                new XElement("visual",
                    binding));
            
            return CreatePushTest(wnsMethod, payload, expectedResult);
        }

        private static ZumoTest CreateBadgePushTest(object badgeValue, int? version = null)
        {
            IJsonValue badge;
            if (badgeValue is int)
            {
                badge = JsonValue.CreateNumberValue((int)badgeValue);
            }
            else if (badgeValue is string)
            {
                badge = JsonValue.CreateStringValue((string)badgeValue);
            }
            else
            {
                throw new ArgumentException("Invalid badge value: " + badgeValue);
            }

            if (version.HasValue)
            {
                JsonObject payload = new JsonObject();
                payload.Add("value", badge);
                payload.Add("version", JsonValue.CreateNumberValue(version.Value));
                badge = payload;
            }

            XElement expected = new XElement("badge",
                new XAttribute("value", badgeValue),
                new XAttribute("version", version.HasValue ? version.GetValueOrDefault() : 1));

            return CreatePushTest("sendBadge", badge, expected);
        }

        private static void AddIfNotNull(JsonObject obj, string name, string value)
        {
            if (value != null)
            {
                obj.Add(name, JsonValue.CreateStringValue((string)value));
            }
        }

        private static ZumoTest CreatePushTest(string wnsMethod, IJsonValue payload, XElement expectedResult)
        {
            string testName = "Test for " + wnsMethod + ": ";
            string payloadString = payload.Stringify();
            testName += payloadString.Length < 15 ? payloadString : (payloadString.Substring(0, 15) + "...");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                test.AddLog("Test for method {0}, with payload {1}", wnsMethod, payload.Stringify());
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);
                PushWatcher watcher = new PushWatcher();
                var item = new JsonObject();
                item.Add("method", JsonValue.CreateStringValue(wnsMethod));
                item.Add("channelUri", JsonValue.CreateStringValue(pushChannel.Uri));
                item.Add("payload", payload);
                var pushResult = await table.InsertAsync(item);
                test.AddLog("Push result: {0}", pushResult.Stringify());
                var notificationResult = await watcher.WaitForPush(TimeSpan.FromSeconds(10));
                if (notificationResult == null)
                {
                    test.AddLog("Error, push not received on the timeout allowed");
                    return false;
                }
                else
                {
                    test.AddLog("Push notification received:");
                    XElement receivedPushInfo = null;
                    switch (notificationResult.NotificationType)
                    {
                        case PushNotificationType.Raw:
                            receivedPushInfo = new XElement("raw", new XText(notificationResult.RawNotification.Content));
                            break;
                        case PushNotificationType.Toast:
                            receivedPushInfo = XElement.Parse(notificationResult.ToastNotification.Content.GetXml());
                            break;
                        case PushNotificationType.Badge:
                            receivedPushInfo = XElement.Parse(notificationResult.BadgeNotification.Content.GetXml());
                            break;
                        case PushNotificationType.Tile:
                            receivedPushInfo = XElement.Parse(notificationResult.TileNotification.Content.GetXml());
                            break;
                    }

                    test.AddLog("  {0}: {1}", notificationResult.NotificationType, receivedPushInfo);

                    bool passed;
                    if (expectedResult.ToString(SaveOptions.DisableFormatting) == receivedPushInfo.ToString(SaveOptions.DisableFormatting))
                    {
                        test.AddLog("Received notification is the expected one.");
                        passed = true;
                    }
                    else
                    {
                        test.AddLog("Received notification is not the expected one. Expected:");
                        test.AddLog(expectedResult.ToString());
                        test.AddLog("Actual:");
                        test.AddLog(receivedPushInfo.ToString());
                        passed = false;
                    }

                    await Task.Delay(5000); // leave some time between pushes
                    return passed;
                }
            });
        }

        private static ZumoTest CreateRegisterChannelTest()
        {
            return new ZumoTest("Register push channel", async delegate(ZumoTest test)
            {
                ZumoPushTests.pushChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                test.AddLog("Registered the channel; uri = {0}", pushChannel.Uri);
                pushChannel.PushNotificationReceived += pushChannel_PushNotificationReceived;
                return true;
            });
        }

        private static ZumoTest CreateUnregisterChannelTest()
        {
            return new ZumoTest("Unregister push channel", delegate(ZumoTest test)
            {
                pushChannel.Close();
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            });
        }

        static void pushChannel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            pushesReceived.Enqueue(args);
        }

        class PushWatcher
        {
            public async Task<PushNotificationReceivedEventArgs> WaitForPush(TimeSpan maximumWait)
            {
                PushNotificationReceivedEventArgs result = null;
                var tcs = new TaskCompletionSource<PushNotificationReceivedEventArgs>();
                DateTime start = DateTime.UtcNow;
                while (DateTime.UtcNow.Subtract(start) < maximumWait)
                {
                    if (pushesReceived.Count > 0)
                    {
                        result = pushesReceived.Dequeue();
                        break;
                    }

                    await Task.Delay(500);
                }

                return result;
            }
        }
    }
}
