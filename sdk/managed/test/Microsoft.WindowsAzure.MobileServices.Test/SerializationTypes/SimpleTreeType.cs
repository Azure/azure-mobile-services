// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{

    public class SimpleTreeType : JsonConverter
    {
        public long Id { get; set; }
        public string Name { get; set; }

        [JsonConverter(typeof(SimpleTreeType))]
        public List<SimpleTreeType> Children { get; set; }

        public SimpleTreeType() : this(false)
        {
        }

        public SimpleTreeType(bool setValues)
        {
            Children = new List<SimpleTreeType>();

            if (setValues)
            {
                this.Id = 5;
                this.Name = "Root";
                this.Children.Add(new SimpleTreeType() { Id = 6, Name = "Child1" });
                this.Children.Add(new SimpleTreeType() { Id = 7, Name = "Child2" });
            }
        }

        public override bool Equals(object obj)
        {
            SimpleTreeType other = obj as SimpleTreeType;
            if (other == null || 
                this.Id != other.Id || 
                this.Name != other.Name ||
                this.Children.Count != other.Children.Count)
            {
                return false;
            }

            for (int i = 0; i < this.Children.Count; i++)
            {
                if (!object.Equals(this.Children[i], other.Children[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool CanConvert(System.Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            List<SimpleTreeType> list = existingValue as List<SimpleTreeType>;
            List<SimpleTreeType> newList = serializer.Deserialize(reader, objectType) as List<SimpleTreeType>;

            list.Clear();
            list.AddRange(newList);
            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
