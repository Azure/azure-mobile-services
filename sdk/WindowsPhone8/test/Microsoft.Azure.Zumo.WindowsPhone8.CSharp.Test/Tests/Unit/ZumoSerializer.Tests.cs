// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Azure.Zumo.WindowsPhone8.Test;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Zumo.WindowsPhone8.CSharp.Test
{
    public class Person
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [DataContract]
    public class Animal
    {
        [DataMember(Name = "id")]
        public int Id;

        [DataMember(Name = "species", IsRequired = true)]
        public string Species { get; set; }

        [DataMember]
        internal bool SoftAndFurry { get; set; }

        public bool Deadly { get; set; }
    }

    public class Hyperlink
    {
        public int ID { get; set; }
        
        [DataMemberJsonConverter(ConverterType = typeof(UriConverter))]
        public Uri Href { get; set; }

        public string Alt { get; set; }

        internal class UriConverter : IDataMemberJsonConverter
        {
            public object ConvertFromJson(JToken value)
            {
                string text = value.AsString();
                return text != null ?
                    new Uri(text) :
                    null;
            }

            public JToken ConvertToJson(object instance)
            {
                Uri uri = instance as Uri;
                return new JValue(
                    uri != null ? uri.ToString() : "");
            }
        }
    }

    public class SimpleTree : ICustomMobileServiceTableSerialization
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [IgnoreDataMember]
        public List<SimpleTree> Children { get; set; }

        public SimpleTree()
        {
            Children = new List<SimpleTree>();
        }


        JToken ICustomMobileServiceTableSerialization.Serialize()
        {
            JArray children = new JArray();
            foreach (SimpleTree child in Children)
            {
                children.Append(MobileServiceTableSerializer.Serialize(child));
            }

            return new JObject()
                .Set("id", Id)
                .Set("name", Name)
                .Set("children", children);
        }

        void ICustomMobileServiceTableSerialization.Deserialize(JToken value)
        {
            int? id = value.Get("id").AsInteger();
            if (id != null)
            {
                Id = id.Value;
            }

            Name = value.Get("name").AsString();

            JArray children = value["children"] as JArray;
            if (children != null)
            {
                Children.AddRange(children.Select(MobileServiceTableSerializer.Deserialize<SimpleTree>));
            }
        }
    }
    
    public class ZumoSerializerTests : TestBase
    {
        [TestMethod]
        public void BasicSerialization()
        {
            // Serialize an instance without an ID
            JToken value = MobileServiceTableSerializer.Serialize(
                new Person { Name = "John", Age = 45 });
            Assert.AreEqual("John", value.Get("Name").AsString());
            Assert.AreEqual(45, value.Get("Age").AsInteger());
            Assert.IsNull(value.Get("id").AsInteger());

            // Ensure the ID is written when provided
            // Serialize an instance without an ID
            value = MobileServiceTableSerializer.Serialize(
                new Person { Id = 2, Name = "John", Age = 45 });
            Assert.AreEqual("John", value.Get("Name").AsString());
            Assert.AreEqual(45, value.Get("Age").AsInteger());
            Assert.AreEqual(2, value.Get("id").AsInteger());

            // Ensure other properties are included but null
            value = MobileServiceTableSerializer.Serialize(new Person { Id = 12 });
            string text = value.ToString();
            Assert.Contains(text, "Name");
            Assert.Contains(text, "Age");

            Throws<ArgumentNullException>(() => MobileServiceTableSerializer.Serialize(null));
        }

        [TestMethod]
        public void BasicDeserialization()
        {
            // Create a new instnace
            Person p = MobileServiceTableSerializer.Deserialize<Person>(
                new JObject()
                    .Set("Name", "Bob")
                    .Set("Age", 20)
                    .Set("id", 10));
            Assert.AreEqual(10L, p.Id);
            Assert.AreEqual("Bob", p.Name);
            Assert.AreEqual(20, p.Age);

            // Deserialize into the same instance
            MobileServiceTableSerializer.Deserialize(
                new JObject()
                    .Set("Age", 21)
                    .Set("Name", "Roberto"),
                p);
            Assert.AreEqual(10L, p.Id);
            Assert.AreEqual("Roberto", p.Name);
            Assert.AreEqual(21, p.Age);

            Throws<ArgumentNullException>(() => MobileServiceTableSerializer.Deserialize(null, new object()));
            Throws<ArgumentNullException>(() => MobileServiceTableSerializer.Deserialize(JsonExtensions.Null(), null));
        }

        [TestMethod]
        public void DataContractSerialization()
        {
            // Serialize a type with a data contract
            Animal bear = new Animal { Id = 1, Species = "Grizzly", Deadly = true, SoftAndFurry = true};
            JToken value = MobileServiceTableSerializer.Serialize(bear);
            Assert.AreEqual(1, value.Get("id").AsInteger());
            Assert.AreEqual("Grizzly", value.Get("species").AsString());
            Assert.IsTrue(value.Get("SoftAndFurry").AsBool().Value);
            Assert.IsTrue(value.Get("Deadly").IsNull());

            // Deserialize a type with a data contract
            Animal donkey = MobileServiceTableSerializer.Deserialize<Animal>(
                new JObject()
                    .Set("id", 2)
                    .Set("species", "Stubbornus Maximums")
                    .Set("Deadly", true)
                    .Set("SoftAndFurry", false));
            Assert.AreEqual(2, donkey.Id);
            Assert.AreEqual("Stubbornus Maximums", donkey.Species);
            Assert.IsFalse(donkey.SoftAndFurry);
            Assert.IsFalse(donkey.Deadly); // No DataMember so not serialized

            // Ensure we throw if we're missing a required
            Throws<SerializationException>(() => MobileServiceTableSerializer.Deserialize<Animal>(
                new JObject().Set("id", 3).Set("name", "Pterodactyl").Set("Deadly", true)));
        }

        [TestMethod]
        public void DataMemberJsonConverter()
        {
            // Serialize with a custom JSON converter
            JToken link = MobileServiceTableSerializer.Serialize(
                new Hyperlink
                {
                    Href = new Uri("http://www.microsoft.com/"),
                    Alt = "Microsoft"
                });
            Assert.AreEqual("Microsoft", link.Get("Alt").AsString());
            Assert.AreEqual("http://www.microsoft.com/", link.Get("Href").AsString());

            // Deserialize with a custom JSON converter
            Hyperlink azure = MobileServiceTableSerializer.Deserialize<Hyperlink>(
                new JObject().Set("Alt", "Windows Azure").Set("Href", "http://windowsazure.com"));
            Assert.AreEqual("Windows Azure", azure.Alt);
            Assert.AreEqual("windowsazure.com", azure.Href.Host);
        }

        [TestMethod]
        public void CustomSerialization()
        {
            SimpleTree tree = new SimpleTree
            {
                Id = 1,
                Name = "John",
                Children = new List<SimpleTree>
                {
                    new SimpleTree { Id = 2, Name = "James" },
                    new SimpleTree
                    {
                        Id = 3,
                        Name = "David",
                        Children = new List<SimpleTree>
                        {
                            new SimpleTree { Id = 4, Name = "Jennifer" }
                        }
                    }
                }
            };

            JToken family = MobileServiceTableSerializer.Serialize(tree);
            Assert.AreEqual("Jennifer",
                family.Get("children").AsArray()[1].Get("children").AsArray()[0].Get("name").AsString());

            SimpleTree second = MobileServiceTableSerializer.Deserialize<SimpleTree>(family);
            Assert.AreEqual(tree.Children[0].Name, second.Children[0].Name);
        }
    }
}
