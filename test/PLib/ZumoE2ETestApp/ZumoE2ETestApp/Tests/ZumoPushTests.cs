// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Windows.Networking.PushNotifications;
using ZumoE2ETestApp.Framework;
using System.Net.Http;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoPushTests
    {
        private static string pushChannelUri;
        private static PushNotificationChannel pushChannel;
        private static Queue<PushNotificationReceivedEventArgs> pushesReceived = new Queue<PushNotificationReceivedEventArgs>();
        const string imageUrl = "http://zumotestserver.azurewebsites.net/content/zumo2.png";
        const string wideImageUrl = "http://zumotestserver.azurewebsites.net/content/zumo1.png";

        public static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Push tests");
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
            result.AddTest(CreateRegisterTemplateChannelTest("Toast"));
            result.AddTest(CreateTemplateToastPushTest("sendToastText01", "World News in English!"));
            result.AddTest(CreateUnregisterTemplateChannelTest(ZumoPushTestGlobals.NHToastTemplateName));
            result.AddTest(CreateRegisterTemplateChannelTest("Tile"));
            result.AddTest(CreateTemplateTilePushTest("TileWideImageAndText02", new[] { "tl-wiat2-1", "在普通话的世界新闻！" }, new[] { wideImageUrl }, new[] { "zumowide" }));
            result.AddTest(CreateUnregisterTemplateChannelTest(ZumoPushTestGlobals.NHTileTemplateName));
            result.AddTest(CreateRegisterTemplateChannelTest("Badge"));
            result.AddTest(CreateTemplateBadgePushTest("10"));
            result.AddTest(CreateUnregisterTemplateChannelTest(ZumoPushTestGlobals.NHBadgeTemplateName));
            result.AddTest(CreateRegisterTemplateChannelTest("Raw"));
            result.AddTest(CreateTemplateRawPushTest("Nouvelles du monde en français!"));
            result.AddTest(CreateUnregisterTemplateChannelTest(ZumoPushTestGlobals.NHRawTemplateName));

            return result;
        }

        private static ZumoTest CreateRawPushTest(string rawData)
        {
            XElement expected = new XElement("raw", new XText(rawData));
            JToken payload = rawData;
            return CreatePushTest("sendRaw", "raw", payload, expected);
        }

        private static ZumoTest CreateTemplateRawPushTest(string rawData)
        {
            XElement expected = new XElement("raw", new XText(rawData));
            JToken payload = rawData;
            return CreatePushTest("sendRaw", "template", payload, expected, true);
        }

        private static ZumoTest CreateTilePushTest(string template, string[] texts, string[] imageUrls, string[] imageAlts)
        {
            var payload = BuildTilPayload(template, texts, imageUrls, imageAlts);
            XElement expected = BuildXmlTilePayload(template, texts, imageUrls, imageAlts);
            return CreatePushTest("send" + template, "tile", payload, expected);
        }

        private static ZumoTest CreateTemplateTilePushTest(string template, string[] texts, string[] imageUrls, string[] imageAlts)
        {
            var payload = BuildTilPayload(template, texts, imageUrls, imageAlts);
            XElement expected = BuildXmlTilePayload(template, texts, imageUrls, imageAlts);
            return CreatePushTest("send" + template, "template", payload, expected, true);
        }

        private static JObject BuildTilPayload(string template, string[] texts, string[] imageUrls, string[] imageAlts)
        {
            if (imageAlts.Length != imageAlts.Length)
            {
                throw new ArgumentException("Size of 'imageUrls' and 'imageAlts' arrays must be the same");
            }

            if (texts.Any(t => t == null) || imageAlts.Any(i => i == null) || imageUrls.Any(i => i == null))
            {
                throw new ArgumentException("No nulls allowed in the arrays");
            }
            var payload = new JObject();
            for (int i = 0; i < imageAlts.Length; i++)
            {
                payload.Add("image" + (i + 1) + "src", imageUrls[i]);
                payload.Add("image" + (i + 1) + "alt", imageAlts[i]);
            }

            for (int i = 0; i < texts.Length; i++)
            {
                payload.Add("text" + (i + 1), texts[i]);
            }

            return payload;
        }

        private static XElement BuildXmlTilePayload(string template, string[] texts, string[] imageUrls, string[] imageAlts)
        {
            XElement binding = new XElement("binding", new XAttribute("template", template));

            for (int i = 0; i < imageAlts.Length; i++)
            {
                binding.Add(new XElement("image",
                new XAttribute("id", (i + 1)),
                new XAttribute("src", imageUrls[i]),
                new XAttribute("alt", imageAlts[i])));
            }

            for (int i = 0; i < texts.Length; i++)
            {
                binding.Add(new XElement("text", new XAttribute("id", (i + 1)), new XText(texts[i])));
            }

            XElement xmlPayload = new XElement("tile",
                new XElement("visual",
                    binding));
            return xmlPayload;
        }

        private static ZumoTest CreateToastPushTest(string wnsMethod, string text1, string text2 = null, string text3 = null, string imageUrl = null, string imageAlt = null)
        {
            var payload = new JObject();
            payload.Add("text1", text1);
            AddIfNotNull(payload, "text2", text2);
            AddIfNotNull(payload, "text3", text3);
            AddIfNotNull(payload, "image1src", imageUrl);
            AddIfNotNull(payload, "image1alt", imageAlt);

            XElement expectedResult = BuildXmlToastPayload(wnsMethod, text1, text2, text3, imageUrl, imageAlt);

            return CreatePushTest(wnsMethod, "toast", payload, expectedResult);
        }

        private static ZumoTest CreateTemplateToastPushTest(string wnsMethod, string text1)
        {
            var payload = new JObject();
            payload.Add("text1", text1);
            XElement expectedResult = BuildXmlToastPayload(wnsMethod, text1);

            return CreatePushTest(wnsMethod, "template", payload, expectedResult, true);
        }

        private static XElement BuildXmlToastPayload(string wnsMethod, string text1, string text2 = null, string text3 = null, string imageUrl = null, string imageAlt = null)
        {
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

            XElement xmlPayload = new XElement("toast",
                new XElement("visual",
                    binding));
            return xmlPayload;
        }

        private static ZumoTest CreateBadgePushTest(object badgeValue, int? version = null)
        {
            var badge = BuildBadgePayload(badgeValue, version);
            XElement expected = BuildBadgeXmlPayload(badgeValue, version);

            return CreatePushTest("sendBadge", "badge", badge, expected);
        }

        private static ZumoTest CreateTemplateBadgePushTest(object badgeValue, int? version = null)
        {
            var badge = BuildBadgePayload(badgeValue, version);
            XElement expected = BuildBadgeXmlPayload(badgeValue, version);

            return CreatePushTest("sendBadge", "template", badge, expected, true);
        }

        private static JToken BuildBadgePayload(object badgeValue, int? version = null)
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

            return badge;
        }

        private static XElement BuildBadgeXmlPayload(object badgeValue, int? version = null)
        {
            XElement xmlPayload = new XElement("badge",
                new XAttribute("value", badgeValue),
                new XAttribute("version", version.HasValue ? version.GetValueOrDefault() : 1));
            return xmlPayload;
        }

        private static void AddIfNotNull(JObject obj, string name, string value)
        {
            if (value != null)
            {
                obj.Add(name, (string)value);
            }
        }
        private static ZumoTest CreatePushTest(string wnsMethod, string nhNotificationType, JToken payload, XElement expectedResult, bool templatePush = false)
        {
            string testName = "Test for " + wnsMethod + ": ";
            string payloadString = payload.ToString(Formatting.None);
            testName += payloadString.Length < 15 ? payloadString : (payloadString.Substring(0, 15) + "...");
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                test.AddLog("Test for method {0}, with payload {1}", wnsMethod, payload);
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.PushTestTableName);

                // Workaround for multiple registration bug
                ZumoPushTests.pushesReceived.Clear();

                PushWatcher watcher = new PushWatcher();
                var item = new JObject();
                item.Add("method", wnsMethod);
                item.Add("channelUri", pushChannelUri);
                item.Add("payload", payload);
                item.Add("xmlPayload", expectedResult.ToString());
                item.Add("templateNotification", ZumoPushTestGlobals.TemplateNotification);
                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    item.Add("usingNH", true);
                    item.Add("nhNotificationType", nhNotificationType);
                }
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
                            if (nhNotificationType == "template")
                            {
                                receivedPushInfo = XElement.Parse(notificationResult.RawNotification.Content);
                            }
                            else
                            {
                                receivedPushInfo = new XElement("raw", new XText(notificationResult.RawNotification.Content));
                            }
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
            }, templatePush ? ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED : ZumoTestGlobals.RuntimeFeatureNames.STRING_ID_TABLES);
        }

        private static ZumoTest CreateRegisterChannelTest()
        {
            return new ZumoTest("Register push channel", async delegate(ZumoTest test)
            {
                ZumoPushTests.pushChannel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
                test.AddLog("Register the channel; uri = {0}", pushChannel.Uri);
                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    var client = ZumoTestGlobals.Instance.Client;
                    var push = client.GetPush();
                    await push.RegisterNativeAsync(ZumoPushTests.pushChannel.Uri, "tag1 tag2".Split());
                    pushChannelUri = null;
                    test.AddLog("RegisterNative with NH succeeded.");
                }
                else
                {
                    pushChannelUri = pushChannel.Uri;
                    test.AddLog("Register succeeded.");
                }

                pushChannel.PushNotificationReceived += pushChannel_PushNotificationReceived;
                return true;
            });
        }

        private static ZumoTest CreateRegisterTemplateChannelTest(string nhNotificationType)
        {
            return new ZumoTest("Register template " + nhNotificationType + " push channel", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var push = client.GetPush();
                WnsTemplateRegistration reg = null;
                switch (nhNotificationType.ToLower())
                {
                    case "toast":
                        var toastTemplate = BuildXmlToastPayload("sendToastText01", "$(News_English)");
                        reg = new WnsTemplateRegistration(ZumoPushTests.pushChannel.Uri, toastTemplate.ToString(), ZumoPushTestGlobals.NHToastTemplateName, "World English".Split());
                        break;
                    case "raw":
                        var rawTemplate = "<raw>$(News_French)</raw>";
                        IDictionary<string, string> wnsHeaders = new Dictionary<string, string>();
                        wnsHeaders.Add("X-WNS-Type", "wns/raw");
                        reg = new WnsTemplateRegistration(ZumoPushTests.pushChannel.Uri, rawTemplate, ZumoPushTestGlobals.NHRawTemplateName, "World Mandarin".Split(), wnsHeaders);
                        break;
                    case "badge":
                        var badgeTemplate = BuildBadgeXmlPayload("$(News_Badge)");
                        reg = new WnsTemplateRegistration(ZumoPushTests.pushChannel.Uri, badgeTemplate.ToString(), ZumoPushTestGlobals.NHBadgeTemplateName, "World Badge".Split());
                        break;
                    case "tile":
                        var tileTemplate = BuildXmlTilePayload("TileWideImageAndText02", new[] { "tl-wiat2-1", "$(News_Mandarin)" }, new[] { wideImageUrl }, new[] { "zumowide" });
                        reg = new WnsTemplateRegistration(ZumoPushTests.pushChannel.Uri, tileTemplate.ToString(), ZumoPushTestGlobals.NHTileTemplateName, "World Mandarin".Split());
                        break;
                    default:
                        throw new Exception("Template type" + nhNotificationType + "is not supported.");
                }

                await push.RegisterAsync(reg);
                pushChannelUri = null;
                test.AddLog("Registered " + nhNotificationType + " template with NH succeeded.");
                pushChannel.PushNotificationReceived += pushChannel_PushNotificationReceived;
                return true;
            }, ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED);
        }

        private static ZumoTest CreateUnregisterChannelTest()
        {
            return new ZumoTest("Unregister push channel", async delegate(ZumoTest test)
            {
                if (ZumoTestGlobals.Instance.IsNHPushEnabled)
                {
                    var client = ZumoTestGlobals.Instance.Client;
                    var push = client.GetPush();
                    await push.UnregisterNativeAsync();
                    test.AddLog("Unregister NH push channel succeeded.");
                }
                else
                {
                    pushChannel.Close();
                    test.AddLog("Unregister push channel succeeded.");
                }
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return await tcs.Task;
            });
        }

        private static ZumoTest CreateUnregisterTemplateChannelTest(string templateName)
        {
            return new ZumoTest("Unregister " + templateName + " push channel", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var push = client.GetPush();
                await push.UnregisterTemplateAsync(templateName);
                test.AddLog("Unregister " + templateName + " push channel succeeded.");
                TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
                tcs.SetResult(true);
                return await tcs.Task;
            }, ZumoTestGlobals.RuntimeFeatureNames.NH_PUSH_ENABLED);
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
