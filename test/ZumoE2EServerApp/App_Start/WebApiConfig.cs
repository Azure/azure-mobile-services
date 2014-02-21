// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using AutoMapper;
using Microsoft.WindowsAzure.Mobile.Service;
using Microsoft.WindowsAzure.Mobile.Service.Config;
using Microsoft.WindowsAzure.Mobile.Service.Security;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using ZumoE2EServerApp.DataObjects;
using ZumoE2EServerApp.Models;

namespace ZumoE2EServerApp
{
    public static class WebApiConfig
    {
        public static void Register()
        {
            ConfigOptions options = new ConfigOptions
            {
                PushAuthorization = AuthorizationLevel.Application,
                DiagnosticsAuthorization = AuthorizationLevel.Anonymous,
            };

            HttpConfiguration config = ServiceConfig.Initialize(new ConfigBuilder(options));


            // Now add any missing connection strings
            IServiceSettingsProvider settingsProvider = config.DependencyResolver.GetServiceSettingsProvider();
            ServiceSettingsDictionary settings = settingsProvider.GetServiceSettings();
            IDictionary environmentVariables = Environment.GetEnvironmentVariables();
            foreach (var conKey in settings.Connections.Keys.ToArray())
            {
                var envKey = environmentVariables.Keys.OfType<string>().FirstOrDefault(p => p == conKey);
                if (!string.IsNullOrEmpty(envKey))
                {
                    settings.Connections[conKey].ConnectionString = (string)environmentVariables[envKey];
                }
            }

            foreach (var setKey in settings.Keys.ToArray())
            {
                var envKey = environmentVariables.Keys.OfType<string>().FirstOrDefault(p => p == setKey);
                if (!string.IsNullOrEmpty(envKey))
                {
                    settings[setKey] = (string)environmentVariables[envKey];
                }
            }

            // Emulate the auth behavior of the server: default is application unless explicitly set.
            config.Properties["MS_IsHosted"] = true;

            config.Formatters.JsonFormatter.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;

            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<RoundTripTableItem, RoundTripTableItemFakeStringId>()
                    .ForMember(dst => dst.ComplexType1, map => map.Ignore())
                    .ForMember(dst => dst.IntId, map => map.MapFrom(src => src.RoundTripTableItemId))
                    .ForMember(dst => dst.Id, map => map.MapFrom(src => SqlFuncs.StringConvert(src.RoundTripTableItemId).Trim()));
                cfg.CreateMap<RoundTripTableItemFakeStringId, RoundTripTableItem>()
                    .ForSourceMember(src => src.ComplexType1, map => map.Ignore())
                    .ForSourceMember(src => src.ComplexType2, map => map.Ignore())
                    .ForMember(dst => dst.RoundTripTableItemId, map => map.MapFrom(src => src.Id));
            });

            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SDKClientTestContext>());
        }
    }
}
