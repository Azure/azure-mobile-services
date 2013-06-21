// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            const string wideImageUrl = "http://zumotestserver.azurewebsites.net/content/zumo1.png";
            result.AddTest(CreateRegisterChannelTest());
            result.AddTest(CreateToastPushTest("sendToastText01", "hello world"));
            result.AddTest(CreateToastPushTest("sendToastImageAndText03", "ts-iat3-1", "ts-iat3-2", null, imageUrl, "zumo"));
            result.AddTest(CreateToastPushTest("sendToastImageAndText04", "ts-iat4-1", "ts-iat4-2", "ts-iat4-3", imageUrl, "zumo"));
            result.AddTest(CreateBadgePushTest(4));
            result.AddTest(CreateBadgePushTest("playing"));
            result.AddTest(CreateRawPushTest("hello world"));
            result.AddTest(CreateRawPushTest("foobaráéíóú"));
            result.AddTest(CreateTilePushTest("TileWideImageAndText02", new[] { "tl-wiat2-1", "tl-wiat2-2" }, new[] { wideImageUrl }, new[] { "zumowide" }));
            result.AddTest(CreateTilePushTest("TileWideImageCollection",
                new string[0], 
                new[] { wideImageUrl, imageUrl, imageUrl, imageUrl, imageUrl },
                new[] { "zumowide", "zumo", "zumo", "zumo", "zumo" }));
            result.AddTest(CreateTilePushTest("TileWideText02",
                new[] { "large caption", "tl-wt2-1", "tl-wt2-2", "tl-wt2-3", "tl-wt2-4", "tl-wt2-5", "tl-wt2-6", "tl-wt2-7", "tl-wt2-8" },
                new string[0], new string[0]));
            result.AddTest(CreateTilePushTest("TileSquarePeekImageAndText01",
                new[] { "tl-spiat1-1", "tl-spiat1-2", "tl-spiat1-3", "tl-spiat1-4" },
                new[] { imageUrl }, new[] { "zumo img" }));
            result.AddTest(CreateTilePushTest("TileSquareBlock",
                new[] { "24", "aliquam" },
                new string[0], new string[0]));

            result.AddTest(CreateUnregisterChannelTest());
            return result;
        }

        private static ZumoTest CreateRawPushTest(string rawData)
        {
            XElement expected = new XElement("raw", new XText(rawData));
            JToken payload = rawData;
            return CreatePushTest("sendRaw", payload, expected);
        }

        private static ZumoTest CreateTilePushTest(string template, string[] texts, string[] imageUrls, string[] imageAlts)
        {
            if (imageAlts.Length != imageAlts.Length)
            {
                throw new ArgumentException("Size of 'imageUrls' and 'imageAlts' arrays must be the same");
            }

            if (texts.Any(t => t == null) || imageAlts.Any(i => i == null) || imageUrls.Any(i => i == null))
            {
                throw new ArgumentException("No nulls allowed in the arrays");
            }

            XElement binding = new XElement("binding", new XAttribute("template", template));
            var payload = new JObject();

            for (int i = 0; i < imageAlts.Length; i++)
            {
                payload.Add("image" + (i + 1) + "src", imageUrls[i]);
                payload.Add("image" + (i + 1) + "alt", imageAlts[i]);
                binding.Add(new XElement("image",
                    new XAttribute("id", (i + 1)),
                    new XAttribute("src", imageUrls[i]),
                    new XAttribute("alt", imageAlts[i])));
            }

            for (int i = 0; i < texts.Length; i++)
            {
                payload.Add("text" + (i + 1), texts[i]);
                binding.Add(new XElement("text", new XAttribute("id", (i + 1)), new XText(texts[i])));
            }

            XElement expected = new XElement("tile",
                new XElement("visual",
                    binding));

            return CreatePushTest("send" + template, payload, expected);
        }

        private static ZumoTest CreateToastPushTest(string wnsMethod, string text1, string text2 = null, string text3 = null, string imageUrl = null, string imageAlt = null)
        {
            var payload = new JObject();
            payload.Add("text1", text1);
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
                binding.Add(new XElement("text", new XAttribute("id", 3), new XText(text3)));
            }

            XElement expectedResult = new XElement("toast",
                new XElement("visual",
                    binding));
            
            return CreatePushTest(wnsMethod, payload, expectedResult);
        }

        private static ZumoTest CreateBadgePushTest(object badgeValue, int? version = null)
        {
            JToken badge;
            if (badgeValue is int)
            {
                badge = new JValue((int)badgeValue);
            }
            else if (badgeValue is string)
            {
                badge = new JValue((string)badgeValue);
            }
            else
            {
                throw new ArgumentException("Invalid badge value: " + badgeValue);
            }

            if (version.HasValue)
            {
                var payload = new JObject();
                payload.Add("value", badge);
                payload.Add("version", version.Value);
                badge = payload;
            }

            XElement expected = new XElement("badge",
                new XAttribute("value", badgeValue),
                new XAttribute("version", version.HasValue ? version.GetValueOrDefault() : 1));

            return CreatePushTest("sendBadge", badge, expected);
        }

        private static void AddIfNotNull(JObject obj, string name, string value)
        {
            if (value != null)
            {
                obj.Add(name, (string)value);
            }
        }

        private static ZumoTest CreatePushTest(string wnsMethod, JToken payload, XElement expectedResult)
        {
            string testName = "Test for " + wnsMethod + ": ";
            string payloadString = payload.ToString(Formatting.None);
            testName += payloadString.Length < 15 ? payloadString : (payloadString.Substring(0, 15) + "...");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                test.AddLog("Test for method {0}, with payload {1}", wnsMethod, payload);
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);
                PushWatcher watcher = new PushWatcher();
                var item = new JObject();
                item.Add("method", wnsMethod);
                item.Add("channelUri", pushChannel.Uri);
                item.Add("payload", payload);
                var pushResult = await table.InsertAsync(item);
                test.AddLog("Push result: {0}", pushResult);
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
