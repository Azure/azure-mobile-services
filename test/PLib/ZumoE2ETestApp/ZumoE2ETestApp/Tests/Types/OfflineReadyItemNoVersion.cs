﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests.Types
{
    [DataTable(ZumoOfflineTests.OfflineReadyNoVersionAuthenticatedTableName)]
    public class OfflineReadyItemNoVersion
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("float")]
        public double FloatingNumber { get; set; }

        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("bool")]
        public bool Flag { get; set; }

        public OfflineReadyItemNoVersion() { }

        public OfflineReadyItemNoVersion(Random rndGen)
        {
            this.Name = Util.CreateSimpleRandomString(rndGen, 10);
            this.Age = rndGen.Next();
            this.FloatingNumber = rndGen.Next() * rndGen.NextDouble();
            this.Date = new DateTime(rndGen.Next(1980, 2000), rndGen.Next(1, 12), rndGen.Next(1, 25), rndGen.Next(0, 24), rndGen.Next(0, 60), rndGen.Next(0, 60), DateTimeKind.Utc);
            this.Flag = rndGen.Next(2) == 0;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "OfflineItem[Id={0},Name={1},Age={2},FloatingNumber={3},Date={4},Flag={5}]",
                this.Id, this.Name, this.Age, this.FloatingNumber,
                this.Date.ToString("o", CultureInfo.InvariantCulture), this.Flag);
        }

        public override bool Equals(object obj)
        {
            const double acceptableDifference = 1e-6;
            OfflineReadyItemNoVersion other = obj as OfflineReadyItemNoVersion;
            if (other == null) return false;
            if (this.Age != other.Age) return false;
            if (!this.Date.ToUniversalTime().Equals(other.Date.ToUniversalTime())) return false;
            if (this.Flag != other.Flag) return false;
            if (this.Name != other.Name) return false;
            if (Math.Abs(this.FloatingNumber - other.FloatingNumber) > acceptableDifference) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return this.Age.GetHashCode() ^
                this.Date.ToUniversalTime().GetHashCode() ^
                this.Flag.GetHashCode() ^
                this.Name.GetHashCode();
        }
    }
}
