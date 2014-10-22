using System;
using Microsoft.WindowsAzure.Mobile.Service;

namespace ZumoE2EServerApp.DataObjects
{
    public class OfflineReady : EntityData
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public double Float { get; set; }

        public DateTimeOffset Date { get; set; }

        public bool Bool { get; set; }
    }
}