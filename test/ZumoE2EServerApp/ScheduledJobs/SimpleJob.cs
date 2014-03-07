// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ZumoE2EServerApp2
{
    // A simple scheduled job which can be invoked manually by submitting an HTTP
    // POST request to the path "/jobs/simple".

    public class SimpleJob : ScheduledJob
    {
        public override Task ExecuteAsync()
        {
            Services.Log.Info("Hello from scheduled job!");
            return Task.FromResult(true);
        }
    }
}