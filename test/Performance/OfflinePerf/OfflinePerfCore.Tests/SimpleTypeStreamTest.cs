using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfflinePerfCore.Common;

namespace OfflinePerfCore.Tests
{
    [TestClass]
    public class SimpleTypeStreamTest
    {
        [TestMethod]
        public void ZeroEntriesNoTotalCountYieldsValidJson()
        {
            this.ValidateSimpleTypeStreamYieldsValidJson(0, null);
        }

        [TestMethod]
        public void ZeroEntriesWithTotalCountYieldsValidJson()
        {
            this.ValidateSimpleTypeStreamYieldsValidJson(0, 10000);
        }

        [TestMethod]
        public void MultipleEntriesNoTotalCountYieldsValidJson()
        {
            this.ValidateSimpleTypeStreamYieldsValidJson(10, null);
        }

        [TestMethod]
        public void MultipleEntriesWithTotalCountYieldsValidJson()
        {
            this.ValidateSimpleTypeStreamYieldsValidJson(10, 10000);
        }

        void ValidateSimpleTypeStreamYieldsValidJson(int entryCount, int? totalCount)
        {
            var rndGen = new Random();
            var stream = new SimpleTypeStream(rndGen, entryCount, totalCount);
            var str = new StreamReader(stream).ReadToEnd();
            var json = JToken.Parse(str);
        }
    }
}
