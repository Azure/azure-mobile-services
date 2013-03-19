using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Notification;
using Newtonsoft.Json.Linq;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestAppWP8.Tests
{
    internal static class ZumoPushTests
    {
        private static HttpNotificationChannel pushChannel;
        private static Queue<HttpNotificationEventArgs> rawPushesReceived = new Queue<HttpNotificationEventArgs>();
        private static Queue<NotificationEventArgs> toastTilePushesReceived = new Queue<NotificationEventArgs>();

        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Push tests");
            const string imageUrl = "http://zumotestserver.azurewebsites.net/content/zumo2.png";
            const string wideImageUrl = "http://zumotestserver.azurewebsites.net/content/zumo1.png";
            result.AddTest(CreateRegisterChannelTest());
            result.AddTest(CreateToastPushTest("first text", "second text"));
            result.AddTest(CreateToastPushTest("ãéìôü ÇñÑ", "الكتاب على الطاولة"));
            result.AddTest(CreateToastPushTest("这本书在桌子上", "本は机の上に"));
            result.AddTest(CreateToastPushTest("הספר הוא על השולחן", "Книга лежит на столе"));
            result.AddTest(CreateToastPushTest("with param", "a value", "NewPage"));
            //result.AddTest(CreateToastPushTest("sendToastImageAndText03", "ts-iat3-1", "ts-iat3-2", null, imageUrl, "zumo"));
            //result.AddTest(CreateToastPushTest("sendToastImageAndText04", "ts-iat4-1", "ts-iat4-2", "ts-iat4-3", imageUrl, "zumo"));
            result.AddTest(CreateRawPushTest("hello world"));
            result.AddTest(CreateRawPushTest("foobaráéíóú"));
            //result.AddTest(CreateTilePushTest("TileWideImageAndText02", new[] { "tl-wiat2-1", "tl-wiat2-2" }, new[] { wideImageUrl }, new[] { "zumowide" }));
            //result.AddTest(CreateTilePushTest("TileWideImageCollection",
            //    new string[0],
            //    new[] { wideImageUrl, imageUrl, imageUrl, imageUrl, imageUrl },
            //    new[] { "zumowide", "zumo", "zumo", "zumo", "zumo" }));
            //result.AddTest(CreateTilePushTest("TileWideText02",
            //    new[] { "large caption", "tl-wt2-1", "tl-wt2-2", "tl-wt2-3", "tl-wt2-4", "tl-wt2-5", "tl-wt2-6", "tl-wt2-7", "tl-wt2-8" },
            //    new string[0], new string[0]));
            //result.AddTest(CreateTilePushTest("TileSquarePeekImageAndText01",
            //    new[] { "tl-spiat1-1", "tl-spiat1-2", "tl-spiat1-3", "tl-spiat1-4" },
            //    new[] { imageUrl }, new[] { "zumo img" }));
            //result.AddTest(CreateTilePushTest("TileSquareBlock",
            //    new[] { "24", "aliquam" },
            //    new string[0], new string[0]));

            result.AddTest(CreateUnregisterChannelTest());
            return result;
        }

        private static ZumoTest CreateToastPushTest(string text1, string text2, string param = null)
        {
            var testName = "SendToast - [" + text1 + ", " + text2 + ", " + (param ?? "<<null>>") + "]";
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);
                var item = new JObject();
                item.Add("method", "sendToast");
                item.Add("channelUri", ZumoPushTests.pushChannel.ChannelUri.AbsoluteUri);
                var payload = new JObject();
                payload.Add("text1", text1);
                payload.Add("text2", text2);
                if (param != null)
                {
                    payload.Add("param", param);
                }

                item.Add("payload", payload);
                var response = await table.InsertAsync(item);
                test.AddLog("Response to (virtual) insert for push: {0}", response);
                test.AddLog("Waiting for push...");
                var notification = await WaitForToastTileNotification(TimeSpan.FromSeconds(10));
                if (notification != null)
                {
                    test.AddLog("Received notification:");
                    foreach (var key in notification.Keys)
                    {
                        test.AddLog("  {0}: {1}", key, notification[key]);
                    }

                    return true;
                }
                else
                {
                    test.AddLog("Did not receive notification on time");
                    return false;
                }
            });
        }

        private async static Task<IDictionary<string, string>> WaitForToastTileNotification(TimeSpan maximumWait)
        {
            IDictionary<string, string> result = null;
            var tcs = new TaskCompletionSource<IDictionary<string, string>>();
            DateTime start = DateTime.UtcNow;
            while (DateTime.UtcNow.Subtract(start) < maximumWait)
            {
                if (toastTilePushesReceived.Count > 0)
                {
                    result = toastTilePushesReceived.Dequeue().Collection;
                    break;
                }

                await Task.Delay(500);
            }

            return result;
        }

        private static ZumoTest CreateRawPushTest(string rawText)
        {
            return new ZumoTest("SendRaw - " + rawText, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);
                var item = new JObject();
                item.Add("method", "sendRaw");
                item.Add("channelUri", ZumoPushTests.pushChannel.ChannelUri.AbsoluteUri);
                item.Add("payload", rawText);
                var response = await table.InsertAsync(item);
                test.AddLog("Response to (virtual) insert for push: {0}", response);
                test.AddLog("Waiting for push...");
                var notification = await WaitForHttpNotification(TimeSpan.FromSeconds(10));
                if (notification != null)
                {
                    test.AddLog("Received notification... headers:");
                    foreach (var header in notification.Headers.AllKeys)
                    {
                        test.AddLog("  {0}: {1}", header, notification.Headers[header]);
                    }

                    string notificationBody = new StreamReader(notification.Body).ReadToEnd();
                    test.AddLog("Received raw notification: {0}", notificationBody);
                    if (notificationBody == rawText)
                    {
                        test.AddLog("Received expected notification");
                        return true;
                    }
                    else
                    {
                        test.AddLog("Notification received is incorrect!");
                        return false;
                    }
                }
                else
                {
                    test.AddLog("Did not receive the notification on time");
                    return false;
                }
            });
        }

        private async static Task<HttpNotification> WaitForHttpNotification(TimeSpan maximumWait)
        {
            HttpNotification result = null;
            var tcs = new TaskCompletionSource<HttpNotification>();
            DateTime start = DateTime.UtcNow;
            while (DateTime.UtcNow.Subtract(start) < maximumWait)
            {
                if (rawPushesReceived.Count > 0)
                {
                    result = rawPushesReceived.Dequeue().Notification;
                    break;
                }

                await Task.Delay(500);
            }

            return result;
        }

        private static ZumoTest CreateUnregisterChannelTest()
        {
            return new ZumoTest("Unregister push channel", delegate(ZumoTest test)
            {
                if (ZumoPushTests.pushChannel != null)
                {
                    ZumoPushTests.pushChannel.HttpNotificationReceived -= pushChannel_HttpNotificationReceived;
                    ZumoPushTests.pushChannel.ShellToastNotificationReceived -= pushChannel_ShellToastNotificationReceived;
                    ZumoPushTests.pushChannel.UnbindToShellTile();
                    ZumoPushTests.pushChannel.UnbindToShellToast();
                    test.AddLog("Unbound from shell tile/toast");
                    ZumoPushTests.pushChannel.Close();
                    test.AddLog("Closed the push channel");
                    ZumoPushTests.pushChannel = null;
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return tcs.Task;
            });
        }

        private static ZumoTest CreateRegisterChannelTest()
        {
            return new ZumoTest("Register push channel", async delegate(ZumoTest test)
            {
                string channelName = "MyPushChannel";
                var pushChannel = HttpNotificationChannel.Find(channelName);
                if (pushChannel == null)
                {
                    pushChannel = new HttpNotificationChannel(channelName);
                    test.AddLog("Created new channel");
                }
                else
                {
                    test.AddLog("Reusing existing channel");
                }

                if (pushChannel.ConnectionStatus == ChannelConnectionStatus.Disconnected)
                {
                    pushChannel.Open();
                    test.AddLog("Opened the push channel");
                } else {
                    test.AddLog("Channel already opened");
                }

                if (pushChannel.IsShellToastBound)
                {
                    test.AddLog("Channel is already bound to shell toast");
                }
                else
                {
                    pushChannel.BindToShellTile();
                    pushChannel.BindToShellToast();
                    test.AddLog("Bound the push channel to shell toast / tile");
                }

                pushChannel.HttpNotificationReceived += pushChannel_HttpNotificationReceived;
                pushChannel.ShellToastNotificationReceived += pushChannel_ShellToastNotificationReceived;
                test.AddLog("Registered to raw / shell toast events");

                ZumoPushTests.pushChannel = pushChannel;

                DateTime start = DateTime.UtcNow;
                TimeSpan maxWait = TimeSpan.FromSeconds(10);
                while (DateTime.UtcNow.Subtract(start) < maxWait)
                {
                    if (pushChannel.ConnectionStatus == ChannelConnectionStatus.Connected && pushChannel.ChannelUri != null)
                    {
                        test.AddLog("Channel URI: {0}", pushChannel.ChannelUri);
                        break;
                    }
                    else
                    {
                        test.AddLog("Waiting for the push channel URI to be assigned");
                    }

                    await Task.Delay(500);
                }

                if (pushChannel.ConnectionStatus != ChannelConnectionStatus.Connected || pushChannel.ChannelUri == null)
                {
                    test.AddLog("Error, push channel isn't connected or channel URI is null");
                    return false;
                }
                else
                {
                    return true;
                }
            });
        }

        static void pushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            ZumoPushTests.toastTilePushesReceived.Enqueue(e);
        }

        static void pushChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            ZumoPushTests.rawPushesReceived.Enqueue(e);
        }
    }
}
