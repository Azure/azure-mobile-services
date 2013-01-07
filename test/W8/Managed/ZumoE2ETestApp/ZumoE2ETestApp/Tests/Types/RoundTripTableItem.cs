using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using ZumoE2ETestApp.Framework;

namespace ZumoE2ETestApp.Tests.Types
{
    [DataTable(Name = ZumoTestGlobals.RoundTripTableName)]
    public class RoundTripTableItem
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "string1")]
        public string String1 { get; set; }

        [DataMember(Name = "date1")]
        public DateTime? Date1 { get; set; }

        [DataMember(Name = "bool1")]
        public bool? Bool1 { get; set; }

        // Number types
        [DataMember(Name = "double1")]
        public double Double1 { get; set; }
        [DataMember(Name = "long1")]
        public long Long1 { get; set; }
        [DataMember(Name = "int1")]
        public int? Int1 { get; set; }

        // Enum, with converter
        [DataMemberJsonConverter(ConverterType = typeof(EnumTypeConverter<EnumType>))]
        [DataMember(Name = "enumType1")]
        public EnumType EnumType { get; set; }

        // Complex type, with converter
        [DataMember(Name = "complexType1")]
        [DataMemberJsonConverter(ConverterType = typeof(ComplexTypeArrayConverter))]
        public ComplexType[] ComplexType1 { get; set; }

        // Complex type, ICustomMobileServiceTableSerialization
        [DataMember(Name = "complexType2")]
        [DataMemberJsonConverter(ConverterType = typeof(CustomMobileServiceTableSerializationConverter<ComplexType2>))]
        public ComplexType2 ComplexType2 { get; set; }

        public RoundTripTableItem Clone()
        {
            RoundTripTableItem result = new RoundTripTableItem
            {
                Bool1 = this.Bool1,
                Date1 = this.Date1,
                Double1 = this.Double1,
                EnumType = this.EnumType,
                Int1 = this.Int1,
                Long1 = this.Long1,
                String1 = this.String1,
            };

            if (this.ComplexType1 != null)
            {
                result.ComplexType1 = this.ComplexType1.Select(ct => ct == null ? null : ct.Clone()).ToArray();
            }

            if (this.ComplexType2 != null)
            {
                result.ComplexType2 = this.ComplexType2.Clone();
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            const double acceptableDifference = 1e-6;
            RoundTripTableItem other = obj as RoundTripTableItem;
            if (other == null) return false;
            if (!this.Bool1.Equals(other.Bool1)) return false;
            if (!Util.CompareArrays(this.ComplexType1, other.ComplexType1)) return false;
            if ((this.ComplexType2 == null) != (other.ComplexType2 == null)) return false;
            if (this.Date1.HasValue != other.Date1.HasValue) return false;
            if (this.Date1.HasValue && !this.Date1.Value.ToUniversalTime().Equals(other.Date1.Value.ToUniversalTime())) return false;
            if (Math.Abs(this.Double1 - other.Double1) > acceptableDifference) return false;
            if (!this.EnumType.Equals(other.EnumType)) return false;
            if (!this.Int1.Equals(other.Int1)) return false;
            if (!this.Long1.Equals(other.Long1)) return false;
            if (this.String1 != other.String1) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture,
                "RoundTripTableItem[Bool1={0},ComplexType1={1},ComplexType2={2},Date1={3},Double1={4},EnumType={5},Int1={6},Long1={7},String1={8}]",
                Bool1.HasValue ? Bool1.Value.ToString() : "<<NULL>>",
                Util.ArrayToString(ComplexType1),
                ComplexType2 == null ? "<<NULL>>" : ComplexType2.ToString(),
                Date1.HasValue ? Date1.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff") : "<<NULL>>",
                Double1,
                EnumType,
                Int1.HasValue ? Int1.Value.ToString(CultureInfo.InvariantCulture) : "<<NULL>>",
                Long1,
                String1);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Bool1.GetHashCode();
            result ^= Util.GetArrayHashCode(this.ComplexType1);

            if (this.ComplexType2 != null)
            {
                result ^= this.ComplexType2.GetHashCode();
            }

            if (this.Date1.HasValue)
            {
                result ^= this.Date1.Value.ToUniversalTime().GetHashCode();
            }

            result ^= this.Double1.GetHashCode();
            result ^= this.EnumType.GetHashCode();
            result ^= this.Int1.GetHashCode();
            result ^= this.Long1.GetHashCode();

            if (this.String1 != null)
            {
                result ^= this.String1.GetHashCode();
            }

            return result;
        }
    }

    internal class CustomMobileServiceTableSerializationConverter<T> : IDataMemberJsonConverter
        where T : ICustomMobileServiceTableSerialization, new()
    {
        public object ConvertFromJson(IJsonValue value)
        {
            if (value == null || value.ValueType == JsonValueType.Null)
            {
                return null;
            }
            else
            {
                T result = new T();
                result.Deserialize(value);
                return result;
            }
        }

        public IJsonValue ConvertToJson(object instance)
        {
            if (instance == null)
            {
                return JsonValue.Parse("null");
            }
            else
            {
                return ((ICustomMobileServiceTableSerialization)instance).Serialize();
            }
        }
    }

    public class ComplexType2 : ICustomMobileServiceTableSerialization
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string[] Friends { get; set; }

        public ComplexType2() { }
        public ComplexType2(Random rndGen)
        {
            this.Name = Util.CreateSimpleRandomString(rndGen, 10);
            this.Age = rndGen.Next(80);
            this.Friends = Enumerable.Range(0, 5)
                .Select(_ => Util.CreateSimpleRandomString(rndGen, 5))
                .ToArray();
        }

        public override int GetHashCode()
        {
            int result = 0;
            if (this.Name != null) result ^= this.Name.GetHashCode();
            result ^= this.Age.GetHashCode();
            result ^= Util.GetArrayHashCode(this.Friends);
            return result;
        }

        public override bool Equals(object obj)
        {
            ComplexType2 other = obj as ComplexType2;
            if (other == null) return false;
            if (this.Age != other.Age) return false;
            if (this.Name != other.Name) return false;
            if (!Util.CompareArrays(this.Friends, other.Friends)) return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("ComplexType2[Name={0},Age={1},Friends={2}]",
                Name,
                Age,
                Util.ArrayToString(Friends));
        }

        public void Deserialize(IJsonValue value)
        {
            JsonObject jo = value.GetObject();
            this.Name = jo["Name"].GetString();
            this.Age = (int)jo["Age"].GetNumber();
            this.Friends = jo["Friends"].GetArray().Select(jv => jv.GetString()).ToArray();
        }

        public IJsonValue Serialize()
        {
            JsonObject result = new JsonObject();
            result.Add("Name", JsonValue.CreateStringValue(this.Name));
            result.Add("Age", JsonValue.CreateNumberValue(this.Age));
            JsonArray friends = new JsonArray();
            result.Add("Friends", friends);
            foreach (var friend in this.Friends)
            {
                friends.Add(JsonValue.CreateStringValue(friend));
            }

            return result;
        }

        internal ComplexType2 Clone()
        {
            return new ComplexType2
            {
                Name = this.Name,
                Age = this.Age,
                Friends = this.Friends == null ? null : this.Friends.Select(a => a).ToArray()
            };
        }
    }

    public class ComplexType
    {
        public ComplexType() { }
        public ComplexType(Random rndGen)
        {
            this.Name = Util.CreateSimpleRandomString(rndGen, 10);
            this.Age = rndGen.Next(80);
        }

        public string Name { get; set; }
        public int Age { get; set; }

        public override int GetHashCode()
        {
            int result = this.Age.GetHashCode();
            if (this.Name != null)
            {
                result ^= this.Name.GetHashCode();
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            ComplexType other = obj as ComplexType;
            return other != null && this.Age == other.Age && this.Name == other.Name;
        }

        public override string ToString()
        {
            return string.Format("ComplexType[Name={0},Age={1}]", Name, Age);
        }

        internal ComplexType Clone()
        {
            return new ComplexType { Name = this.Name, Age = this.Age };
        }
    }

    internal class ComplexTypeArrayConverter : IDataMemberJsonConverter
    {
        public object ConvertFromJson(IJsonValue value)
        {
            if (value == null || value.ValueType == JsonValueType.Null)
            {
                return null;
            }
            else
            {
                JsonArray array = value.GetArray();
                ComplexType[] result = new ComplexType[array.Count];
                for (int i = 0; i < array.Count; i++)
                {
                    IJsonValue item = array[i];
                    if (item != null && item.ValueType != JsonValueType.Null)
                    {
                        JsonObject obj = item.GetObject();
                        result[i] = new ComplexType
                        {
                            Name = obj["Name"].GetString(),
                            Age = (int)obj["Age"].GetNumber()
                        };
                    }
                }

                return result;
            }
        }

        public IJsonValue ConvertToJson(object instance)
        {
            var array = instance as ComplexType[];
            if (array == null)
            {
                return JsonValue.Parse("null");
            }
            else
            {
                JsonArray result = new JsonArray();
                foreach (var item in array)
                {
                    if (item == null)
                    {
                        result.Add(JsonValue.Parse("null"));
                    }
                    else
                    {
                        JsonObject obj = new JsonObject();
                        obj.Add("Name", JsonValue.CreateStringValue(item.Name));
                        obj.Add("Age", JsonValue.CreateNumberValue(item.Age));
                        result.Add(obj);
                    }
                }

                return result;
            }
        }
    }

    public enum EnumType { First, Second, Third }

    internal class EnumTypeConverter<TEnum> : IDataMemberJsonConverter
    {
        public object ConvertFromJson(IJsonValue value)
        {
            if (value == null || value.ValueType == JsonValueType.Null)
            {
                return null;
            }
            else
            {
                return Enum.Parse(typeof(TEnum), value.GetString());
            }
        }

        public IJsonValue ConvertToJson(object instance)
        {
            if (instance == null)
            {
                return JsonValue.Parse("null");
            }
            else
            {
                return JsonValue.CreateStringValue(instance.ToString());
            }
        }
    }
}
