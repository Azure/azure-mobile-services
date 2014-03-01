// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;
using System;

namespace ZumoE2ETestApp.Framework
{
    class ZumoPushTestGlobals
    {
        public static string NHWp8RawTemplate = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><wp:Notification xmlns:wp=\"WPNotification\"><wp:Toast><wp:Text1>$(News_{0})</wp:Text1></wp:Toast></wp:Notification>", "French");
        public static string NHWp8ToastTemplate = String.Format("<?xml version=\"1.0\" encoding=\"utf-8\"?><wp:Notification xmlns:wp=\"WPNotification\"><wp:Toast><wp:Text1>$(News_{0})</wp:Text1></wp:Toast></wp:Notification>", "English");
        public static string NHWp8TileTemplate = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                                    <wp:Notification xmlns:wp=""WPNotification"">
                                                       <wp:Tile>
                                                          <wp:BackgroundImage>http://zumotestserver.azurewebsites.net/content/zumo2.png</wp:BackgroundImage>
                                                          <wp:Count>count</wp:Count>
                                                          <wp:Title>$(News_Mandarin)</wp:Title>
                                                       </wp:Tile>
                                                    </wp:Notification>";
        public static string NHToastTemplateName = "newsToastTemplate";
        public static string NHTileTemplateName = "newsTileTemplate";
        public static string NHBadgeTemplateName = "newsBadgeTemplate";
        public static string NHRawTemplateName = "newsRawTemplate";
        public static JObject TemplateNotification = new JObject()
        {
            {"News_English", "World News in English!"},
            {"News_French", "Nouvelles du monde en français!"},
            {"News_Mandarin", "在普通话的世界新闻！"},
            {"News_Badge", "10"}
        };
    }
}
