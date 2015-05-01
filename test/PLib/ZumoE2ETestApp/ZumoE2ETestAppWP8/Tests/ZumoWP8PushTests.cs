// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Phone.Notification;
using Newtonsoft.Json.Linq;
using ZumoE2ETestApp.Framework;
using System.Xml.Linq;
using Microsoft.WindowsAzure.MobileServices;
#if WINDOWS_PHONE
using ZumoE2ETestApp.Tests;
#endif

namespace ZumoE2ETestAppWP8.Tests
{
    internal static class ZumoWP8PushTests
    {
        private static HttpNotificationChannel pushChannel;
        private static Queue<HttpNotificationEventArgs> rawPushesReceived = new Queue<HttpNotificationEventArgs>();
        private static Queue<NotificationEventArgs> toastPushesReceived = new Queue<NotificationEventArgs>();
        const string ImageUrlDomain = "http://zumotestserver.azurewebsites.net";
        const string imageUrl = "http://zumotestserver.azurewebsites.net/content/zumo2.png";
        const string wideImageUrl = "http://zumotestserver.azurewebsites.net/content/zumo1.png";

        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Push tests");
            result.AddTest(CreateRegisterChannelTest());
            result.AddTest(CreateToastPushTest("first text", "second text"));
            result.AddTest(CreateToastPushTest("ãéìôü ÇñÑ", "الكتاب على الطاولة"));
            result.AddTest(CreateToastPushTest("这本书在桌子上", "本は机の上に"));
            result.AddTest(CreateToastPushTest("הספר הוא על השולחן", "Книга лежит на столе"));
            result.AddTest(CreateToastPushTest("with param", "a value", "/UIElements/InputDialog"));
            result.AddTest(CreateRawPushTest("hello world"));
            result.AddTest(CreateRawPushTest("foobaráéíóú"));
            result.AddTest(CreateTilePushTest(
                "Simple tile", new Uri("/Assets/Tiles/IconicTileMediumLarge.png", UriKind.Relative), 0,
                "Simple tile",
                new Uri("/Assets/Tiles/IconicTileMediumLarge.png", UriKind.Relative), "Back title", "Back content"));
            result.AddTest(ZumoTestCommon.CreateTestWithSingleAlert("After clicking OK, make sure the application is pinned to the start menu"));
            result.AddTest(ZumoTestCommon.CreateYesNoTest("Is the app in the start menu?", true));
            result.AddTest(CreateTilePushTest("Tile with image", new Uri(imageUrl), 3, "Test title", new Uri(wideImageUrl), "Back title", "Back content"));
            result.AddTest(ZumoTestCommon.CreateYesNoTest("Did the tile change?", true, 3000));
            result.AddTest(CreateFlipTilePushTest("Flip tile",
                new Uri("/Assets/Tiles/FlipCycleTileMedium.png", UriKind.Relative), 5, "Flip title",
                new Uri("/Assets/Tiles/IconicTileSmall.png", UriKind.Relative), "Flip back title", "Flip back content",
                new Uri("/Assets/Tiles/FlipCycleTileSmall.png", UriKind.Relative), new Uri("/Assets/Tiles/FlipCycleTileLarge.png", UriKind.Relative),
                "Flip wide back content", new Uri("/Assets/Tiles/FlipCycleTileLarge.png", UriKind.Relative)));
            result.AddTest(ZumoTestCommon.CreateYesNoTest("Did the tile change?", true, 3000));

            result.AddTest(CreateUnregisterChannelTest());

            result.AddTest(CreateRegisterChannelTest(true, "toast"));
            result.AddTest(CreateToastPushTest("World News in English!", null, null, true));
            result.AddTest(CreateUnregisterChannelTest(true, "wp8" + ZumoPushTestGlobals.NHToastTemplateName));
            result.AddTest(CreateRegisterChannelTest(true, "tile"));
            result.AddTest(CreateTilePushTest("Tile Template", new Uri(imageUrl), 3, "在普通话的世界新闻！", null, null, null, true));
            result.AddTest(CreateUnregisterChannelTest(true, "wp8" + ZumoPushTestGlobals.NHTileTemplateName));
            result.AddTest(CreateRegisterChannelTest(true, "raw"));
            result.AddTest(CreateRawPushTest("Nouvelles du monde en français!", true));
            result.AddTest(CreateUnregisterChannelTest(true, "wp8" + ZumoPushTestGlobals.NHRawTemplateName));

            return result;
        }

        private static ZumoTest CreateTilePushTest(string testName,
            Uri backgroundImage, int? count, string title,
            Uri backBackgroundImage = null, string backTitle = null, string backContent = null, bool templatePush = false)
        {
            return CreateTilePushTest(testName, "sendTile",
                backgroundImage, count, title,
                backBackgroundImage, backTitle, backContent, null, null, null, null, templatePush);
        }

        private static ZumoTest CreateFlipTilePushTest(
            string testName,
            Uri backgroundImage, int? count, string title,
            Uri backBackgroundImage, string backTitle, string backContent,
            Uri smallBackgroundImage, Uri wideBackgroundImage,
            string wideBackContent, Uri wideBackBackgroundImage)
        {
            return CreateTilePushTest(testName, "sendFlipTile",
                backgroundImage, count, title,
                backBackgroundImage, backTitle, backContent,
                smallBackgroundImage, wideBackgroundImage,
                wideBackContent, wideBackBackgroundImage);
        }

        private static ZumoTest CreateTilePushTest(
            string testName, string methodName,
            Uri backgroundImage, int? count, string title,
            Uri backBackgroundImage = null, string backTitle = null, string backContent = null,
            Uri smallBackgroundImage = null, Uri wideBackgroundImage = null,
            string wideBackContent = null, Uri wideBackBackgroundImage = null, bool templatePush = false)
        {
            return new ZumoTest("SendTile - " + testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);
                var item = new JObject();
                item.Add("method", methodName);
                item.Add("channelUri", ZumoWP8PushTests.pushChannel.ChannelUri.AbsoluteUri);
                item.Add("templateNotification", ZumoPushTestGlobals.TemplateNotification);
                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    item.Add("usingNH", true);
                }
                if (templatePush)
                {
                    item.Add("templatePush", true);
                }
                var payload = new JObject();
                payload.Add("backgroundImage", backgroundImage);
                if (count.HasValue)
                {
                    payload.Add("count", count);
                }

                payload.Add("title", title);
                payload.Add("backBackgroundImage", backBackgroundImage);
                payload.Add("backTitle", backTitle);
                payload.Add("backContent", backContent);

                if (smallBackgroundImage != null) payload.Add("smallBackgroundImage", smallBackgroundImage);
                if (wideBackgroundImage != null) payload.Add("wideBackgroundImage", wideBackgroundImage);
                if (wideBackContent != null) payload.Add("wideBackContent", wideBackContent);
                if (wideBackBackgroundImage != null) payload.Add("wideBackBackgroundImage", wideBackBackgroundImage);

                item.Add("payload", payload);
                var response = await table.InsertAsync(item);
                test.AddLog("Response to (virtual) insert for push: {0}", response);
                return true;
            }, templatePush ? ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED : ZumoTestGlobals.RuntimeFeatureNames.STRING_ID_TABLES)
            {
                CanRunUnattended = false
            };
        }

        private static ZumoTest CreateToastPushTest(string text1, string text2 = null, string param = null, bool templatePush = false)
        {
            var testName = "SendToast - [" + text1 + ", " + text2 + ", " + (param ?? "<<null>>") + "]";
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);
                var item = new JObject();
                item.Add("method", "sendToast");
                item.Add("channelUri", ZumoWP8PushTests.pushChannel.ChannelUri.AbsoluteUri);
                item.Add("templateNotification", ZumoPushTestGlobals.TemplateNotification);
                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    item.Add("usingNH", true);
                }
                if (templatePush)
                {
                    item.Add("templatePush", true);
                }
                var payload = new JObject();
                var expectedPushPayload = new JObject();
                payload.Add(new JProperty("text1", text1));
                expectedPushPayload.Add(new JProperty("wp:Text1", text1));
                if (text2 != null)
                {
                    payload.Add(new JProperty("text2", text2));
                    expectedPushPayload.Add(new JProperty("wp:Text2", text2));
                }
                if (param != null)
                {
                    payload.Add("param", param);
                }

                item.Add("payload", payload);
                var response = await table.InsertAsync(item);
                test.AddLog("Response to (virtual) insert for push: {0}", response);
                test.AddLog("Waiting for push...");
                var notification = await WaitForToastNotification(TimeSpan.FromSeconds(10));
                if (notification != null)
                {
                    test.AddLog("Received notification:");
                    JObject actual = new JObject();
                    foreach (var key in notification.Keys)
                    {
                        actual.Add(key, notification[key]);
                        test.AddLog("  {0}: {1}", key, notification[key]);
                    }

                    List<string> errors = new List<string>();
                    if (Util.CompareJson(expectedPushPayload, actual, errors))
                    {
                        return true;
                    }
                    else
                    {
                        test.AddLog("Error, push received isn't the expected value");
                        foreach (var error in errors)
                        {
                            test.AddLog(error);
                        }
                        return false;
                    }
                }
                else
                {
                    test.AddLog("Did not receive notification on time");
                    return false;
                }
            }, templatePush ? ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED : ZumoTestGlobals.RuntimeFeatureNames.STRING_ID_TABLES);
        }

        private async static Task<IDictionary<string, string>> WaitForToastNotification(TimeSpan maximumWait)
        {
            IDictionary<string, string> result = null;
            var tcs = new TaskCompletionSource<IDictionary<string, string>>();
            DateTime start = DateTime.UtcNow;
            while (DateTime.UtcNow.Subtract(start) < maximumWait)
            {
                if (toastPushesReceived.Count > 0)
                {
                    result = toastPushesReceived.Dequeue().Collection;
                    break;
                }

                await Util.TaskDelay(500);
            }

            return result;
        }

        private static ZumoTest CreateRawPushTest(string rawText, bool templatePush = false)
        {
            return new ZumoTest("SendRaw - " + rawText, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);
                var item = new JObject();
                item.Add("method", "sendRaw");
                item.Add("channelUri", ZumoWP8PushTests.pushChannel.ChannelUri.AbsoluteUri);
                item.Add("payload", rawText);
                item.Add("templateNotification", ZumoPushTestGlobals.TemplateNotification);
                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    item.Add("usingNH", true);
                }
                if (templatePush)
                {
                    item.Add("templatePush", true);
                }
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
                    if (templatePush)
                    {
                        XElement rawNotificationContent = XElement.Parse(notificationBody);
                        notificationBody = rawNotificationContent.Value;
                    }
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
            }, templatePush ? ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED : ZumoTestGlobals.RuntimeFeatureNames.STRING_ID_TABLES);
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

                await Util.TaskDelay(500);
            }

            return result;
        }

        private static ZumoTest CreateUnregisterChannelTest(bool unRegisterTemplate = false, string templateName = null)
        {
            return new ZumoTest("Unregister push channel", async delegate(ZumoTest test)
            {
                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    var client = ZumoTestGlobals.Instance.Client;
                    var push = client.GetPush();
                    if (unRegisterTemplate)
                    {
                        await push.UnregisterTemplateAsync(templateName);
                    }
                    else
                    {
                        await push.UnregisterNativeAsync();
                    }
                }

                if (ZumoWP8PushTests.pushChannel != null)
                {
                    ZumoWP8PushTests.pushChannel.HttpNotificationReceived -= pushChannel_HttpNotificationReceived;
                    ZumoWP8PushTests.pushChannel.ShellToastNotificationReceived -= pushChannel_ShellToastNotificationReceived;
                    ZumoWP8PushTests.pushChannel.UnbindToShellTile();
                    ZumoWP8PushTests.pushChannel.UnbindToShellToast();
                    test.AddLog("Unbound from shell tile/toast");
                    ZumoWP8PushTests.pushChannel.Close();
                    test.AddLog("Closed the push channel");
                    ZumoWP8PushTests.pushChannel = null;
                }

                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return await tcs.Task;
            }, unRegisterTemplate ? ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED : null);
        }

        private static ZumoTest CreateRegisterChannelTest(bool registerTemplate = false, string templateType = null)
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

                ZumoWP8PushTests.pushChannel = pushChannel;

                if (pushChannel.ConnectionStatus == ChannelConnectionStatus.Disconnected || pushChannel.ChannelUri == null)
                {
                    pushChannel.Open();
                    test.AddLog("Opened the push channel");
                }
                else
                {
                    test.AddLog("Channel already opened");
                }

                if (pushChannel.IsShellToastBound)
                {
                    test.AddLog("Channel is already bound to shell toast");
                }
                else
                {
                    var uris = new System.Collections.ObjectModel.Collection<Uri>();
                    uris.Add(new Uri(ImageUrlDomain));
                    pushChannel.BindToShellTile(uris);
                    pushChannel.BindToShellToast();
                    test.AddLog("Bound the push channel to shell toast / tile");
                }

                TimeSpan maxWait = TimeSpan.FromSeconds(30);
                await WaitForChannelUriAssignment(test, pushChannel, maxWait);


                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    var zumoPush = ZumoTestGlobals.Instance.Client.GetPush();
                    MpnsTemplateRegistration reg = null;
                    if (registerTemplate)
                    {
                        switch (templateType)
                        {
                            case "toast":
                                reg = new MpnsTemplateRegistration(pushChannel.ChannelUri.ToString(), ZumoPushTestGlobals.NHWp8ToastTemplate, "wp8" + ZumoPushTestGlobals.NHToastTemplateName, "World English".Split());
                                break;
                            case "tile":
                                reg = new MpnsTemplateRegistration(pushChannel.ChannelUri.ToString(), ZumoPushTestGlobals.NHWp8TileTemplate, "wp8" + ZumoPushTestGlobals.NHTileTemplateName, "World Mandarin".Split());
                                break;
                            case "raw":
                                IDictionary<string, string> wp8Headers = new Dictionary<string, string>();
                                wp8Headers.Add("X-NotificationClass", "3");
                                reg = new MpnsTemplateRegistration(pushChannel.ChannelUri.ToString(), ZumoPushTestGlobals.NHWp8RawTemplate, "wp8" + ZumoPushTestGlobals.NHRawTemplateName, "World French".Split(), wp8Headers);
                                break;
                        }

                        await zumoPush.RegisterAsync(reg);
                        pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(async (o, args) =>
                        {
                            await zumoPush.RegisterAsync(reg);
                        });

                        test.AddLog("Registered to Notification hub");
                    }
                    else
                    {
                        await zumoPush.RegisterNativeAsync(pushChannel.ChannelUri.ToString(), "tag1 tag2".Split());
                        pushChannel.ChannelUriUpdated += new EventHandler<NotificationChannelUriEventArgs>(async (o, args) =>
                        {
                            await zumoPush.RegisterNativeAsync(args.ChannelUri.ToString(), "tag1 tag2".Split());
                        });
                        test.AddLog("Registered with NH");
                    }
                }
                pushChannel.HttpNotificationReceived += pushChannel_HttpNotificationReceived;
                pushChannel.ShellToastNotificationReceived += pushChannel_ShellToastNotificationReceived;
                test.AddLog("Registered to raw / shell toast events");

                if (pushChannel.ConnectionStatus != ChannelConnectionStatus.Connected || pushChannel.ChannelUri == null)
                {
                    test.AddLog("Error, push channel isn't connected or channel URI is null");
                    return false;
                }
                else
                {
                    return true;
                }
            }, registerTemplate ? ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED : null);
        }

        static async Task WaitForChannelUriAssignment(ZumoTest test, HttpNotificationChannel pushChannel, TimeSpan maxWait)
        {
            DateTime start = DateTime.UtcNow;
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

                await Util.TaskDelay(500);
            }
        }

        static void pushChannel_ShellToastNotificationReceived(object sender, NotificationEventArgs e)
        {
            ZumoWP8PushTests.toastPushesReceived.Enqueue(e);
        }

        static void pushChannel_HttpNotificationReceived(object sender, HttpNotificationEventArgs e)
        {
            ZumoWP8PushTests.rawPushesReceived.Enqueue(e);
        }
    }
}
