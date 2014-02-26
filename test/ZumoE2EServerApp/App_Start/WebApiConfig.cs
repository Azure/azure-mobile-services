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
using System.Diagnostics;
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

            // Now add any missing connection strings and app settings from the environment.
            // Any envrionment variables found with names that match existing connection
            // string and app setting names will be used to replace the value.
            // This allows the Web.config (which typically would contain secrets) to be
            // checked in, but requires people running the tests to config their environment.
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
                    // While we would like to use ResolveUsing here, for ComplexType1 and 2, 
                    // we cannot because it is incompatable with LINQ queries, which is the
                    // whole point of doing this mapping. Instead use AfterMap below.
                    .ForMember(dst => dst.ComplexType1, map => map.Ignore())
                    .ForMember(dst => dst.ComplexType2, map => map.Ignore())
                    .ForMember(dst => dst.IntId, map => map.MapFrom(src => src.RoundTripTableItemId))
                    .ForMember(dst => dst.Id, map => map.MapFrom(src => SqlFuncs.StringConvert(src.RoundTripTableItemId).Trim()))
                    .AfterMap((src, dst) =>
                    {
                        dst.ComplexType1 = src.ComplexType1Serialized == null ? null : JsonConvert.DeserializeObject<ComplexType[]>(src.ComplexType1Serialized);
                        dst.ComplexType2 = src.ComplexType2Serialized == null ? null : JsonConvert.DeserializeObject<ComplexType2>(src.ComplexType2Serialized);
                    });
                cfg.CreateMap<RoundTripTableItemFakeStringId, RoundTripTableItem>()
                    .ForMember(dst => dst.ComplexType1Serialized, map => map.ResolveUsing(src => (src.ComplexType1 == null ? null : JsonConvert.SerializeObject(src.ComplexType1))))
                    .ForMember(dst => dst.ComplexType2Serialized, map => map.ResolveUsing(src => (src.ComplexType2 == null ? null : JsonConvert.SerializeObject(src.ComplexType2))))
                    .ForMember(dst => dst.RoundTripTableItemId, map => map.MapFrom(src => src.Id));


                cfg.CreateMap<StringIdRoundTripTableItemForDB, StringIdRoundTripTableItem>()
                    .ForMember(dst => dst.Complex, map => map.Ignore())
                    .ForMember(dst => dst.ComplexType, map => map.Ignore())
                    .AfterMap((src, dst) =>
                    {
                        dst.Complex = src.ComplexSerialized == null ? null : JsonConvert.DeserializeObject<string[]>(src.ComplexSerialized);
                        dst.ComplexType = src.ComplexTypeSerialized == null ? null : JsonConvert.DeserializeObject<string[]>(src.ComplexTypeSerialized);
                    });
                cfg.CreateMap<StringIdRoundTripTableItem, StringIdRoundTripTableItemForDB>()
                    .ForMember(dst => dst.ComplexSerialized, map => map.ResolveUsing(src => (src.Complex == null ? null : JsonConvert.SerializeObject(src.Complex))))
                    .ForMember(dst => dst.ComplexTypeSerialized, map => map.ResolveUsing(src => (src.ComplexType == null ? null : JsonConvert.SerializeObject(src.ComplexType))));

                cfg.CreateMap<W8JSRoundTripTableItemForDB, W8JSRoundTripTableItem>()
                    .ForMember(dst => dst.ComplexType, map => map.Ignore())
                    .ForMember(dst => dst.Id, map => map.MapFrom(src => src.W8JSRoundTripTableItemForDBId))
                    .AfterMap((src, dst) =>
                    {
                        dst.ComplexType = src.ComplexTypeSerialized == null ? null : JsonConvert.DeserializeObject<string[]>(src.ComplexTypeSerialized);
                    });
                cfg.CreateMap<W8JSRoundTripTableItem, W8JSRoundTripTableItemForDB>()
                    .ForMember(dst => dst.ComplexTypeSerialized, map => map.ResolveUsing(src => (src.ComplexType == null ? null : JsonConvert.SerializeObject(src.ComplexType))))
                    .ForMember(dst => dst.W8JSRoundTripTableItemForDBId, map => map.MapFrom(src => src.Id));
            });

            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<SDKClientTestContext>());
        }
    }
}
