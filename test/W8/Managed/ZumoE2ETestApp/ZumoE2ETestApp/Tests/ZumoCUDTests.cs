using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using ZumoE2ETestApp.Framework;
using ZumoE2ETestApp.Tests.Types;

namespace ZumoE2ETestApp.Tests
{
    internal static class ZumoCUDTests
    {
        internal static ZumoTestGroup CreateTests()
        {
            ZumoTestGroup result = new ZumoTestGroup("Update / delete tests");

            DateTime now = DateTime.UtcNow;
            int seed = now.Year * 10000 + now.Month * 100 + now.Day;
            Random rndGen = new Random(seed);

            result.AddTest(CreateTypedUpdateTest("Update typed item", new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen)));
            result.AddTest(CreateTypedUpdateTest(
                "Update typed item, setting values to null",
                new RoundTripTableItem(rndGen),
                new RoundTripTableItem(rndGen) { Bool1 = null, ComplexType2 = null, Int1 = null, String1 = null, ComplexType1 = null }));
            result.AddTest(CreateTypedUpdateTest<MobileServiceInvalidOperationException>("(Neg) Update typed item, non-existing item id",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = 1000000000 }, false));
            result.AddTest(CreateTypedUpdateTest<ArgumentException>("(Neg) Update typed item, id = 0",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = 0 }, false));

            string toInsertJsonString = @"{
                ""string1"":""hello"",
                ""bool1"":true,
                ""int1"":-1234,
                ""double1"":123.45,
                ""long1"":1234,
                ""date1"":""2012-12-13T09:23:12.000Z"",
                ""complexType1"":[{""Name"":""John Doe"",""Age"":33}, null],
                ""complexType2"":{""Name"":""John Doe"",""Age"":33,""Friends"":null}
            }";

            string toUpdateJsonString = @"{
                ""string1"":""world"",
                ""bool1"":false,
                ""int1"":9999,
                ""double1"":888.88,
                ""long1"":77777777,
                ""date1"":""1999-05-23T19:15:54.000Z"",
                ""complexType1"":[{""Name"":""Jane Roe"",""Age"":23}, null],
                ""complexType2"":{""Name"":""Jane Roe"",""Age"":23,""Friends"":null}
            }";

            result.AddTest(CreateUntypedUpdateTest("Update typed item", JsonObject.Parse(toInsertJsonString), JsonObject.Parse(toUpdateJsonString)));

            JsonValue nullValue = JsonValue.Parse("null");
            JsonObject toUpdate = JsonObject.Parse(toUpdateJsonString);
            toUpdate["string1"] = nullValue;
            toUpdate["bool1"] = nullValue;
            toUpdate["complexType2"] = nullValue;
            toUpdate["complexType1"] = nullValue;
            toUpdate["int1"] = nullValue;
            result.AddTest(CreateUntypedUpdateTest("Update typed item, setting values to null", JsonObject.Parse(toInsertJsonString), toUpdate));

            toUpdate["id"] = JsonValue.CreateNumberValue(1000000000);
            result.AddTest(CreateUntypedUpdateTest<ArgumentException>("(Neg) Update typed item, non-existing item id",
                JsonObject.Parse(toInsertJsonString), JsonObject.Parse(toUpdateJsonString), false));

            toUpdate["id"] = JsonValue.CreateNumberValue(0);
            result.AddTest(CreateUntypedUpdateTest<ArgumentException>("(Neg) Update typed item, id = 0",
                JsonObject.Parse(toInsertJsonString), JsonObject.Parse(toUpdateJsonString), false));

            // Delete tests
            result.AddTest(CreateDeleteTest("Delete typed item", true, DeleteTestType.ValidDelete));
            result.AddTest(CreateDeleteTest("(Neg) Delete typed item with non-existing id", true, DeleteTestType.NonExistingId));
            result.AddTest(CreateDeleteTest("Delete untyped item", false, DeleteTestType.ValidDelete));
            result.AddTest(CreateDeleteTest("(Neg) Delete untyped item with non-existing id", false, DeleteTestType.NonExistingId));
            result.AddTest(CreateDeleteTest("(Neg) Delete untyped item without id field", false, DeleteTestType.NoIdField));

            return result;
        }

        enum DeleteTestType { ValidDelete, NonExistingId, NoIdField }

        private static ZumoTest CreateDeleteTest(string testName, bool useTypedTable, DeleteTestType testType)
        {
            if (useTypedTable && testType == DeleteTestType.NoIdField)
            {
                throw new ArgumentException("Cannot send a delete request without an id field on a typed table.");
            }

            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var typedTable = client.GetTable<RoundTripTableItem>();
                var untypedTable = client.GetTable(ZumoTestGlobals.RoundTripTableName);
                RoundTripTableItem itemToInsert = new RoundTripTableItem
                {
                    String1 = "hello",
                    Bool1 = true,
                    Int1 = 123,
                };
                await typedTable.InsertAsync(itemToInsert);
                test.AddLog("Inserted item to be deleted");
                int id = itemToInsert.Id;
                switch (testType)
                {
                    case DeleteTestType.ValidDelete:
                        if (useTypedTable)
                        {
                            await typedTable.DeleteAsync(itemToInsert);
                        }
                        else
                        {
                            await untypedTable.DeleteAsync(JsonObject.Parse("{\"id\":" + id + "}"));
                        }

                        test.AddLog("Delete succeeded; verifying that object isn't in the service anymore.");
                        try
                        {
                            var response = await untypedTable.LookupAsync(id);
                            test.AddLog("Error, delete succeeded, but item was returned by the service: {0}", response.Stringify());
                            return false;
                        }
                        catch (MobileServiceInvalidOperationException msioe)
                        {
                            return Validate404Response(test, msioe);
                        }

                    case DeleteTestType.NonExistingId:
                        try
                        {
                            if (useTypedTable)
                            {
                                itemToInsert.Id = 1000000000;
                                await typedTable.DeleteAsync(itemToInsert);
                            }
                            else
                            {
                                JsonObject jo = new JsonObject();
                                jo.Add("id", JsonValue.CreateNumberValue(1000000000));
                                await untypedTable.DeleteAsync(jo);
                            }

                            test.AddLog("Error, deleting item with non-existing id should fail, but succeeded");
                            return false;
                        }
                        catch (MobileServiceInvalidOperationException msioe)
                        {
                            return Validate404Response(test, msioe);
                        }

                    default:
                        try
                        {
                            JsonObject jo = new JsonObject();
                            jo.Add("Name", JsonValue.CreateStringValue("hello"));
                            await untypedTable.DeleteAsync(jo);

                            test.AddLog("Error, deleting item with non-existing id should fail, but succeeded");
                            return false;
                        }
                        catch (ArgumentException ex)
                        {
                            test.AddLog("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                            return true;
                        }
                }
            });
        }

        private static bool Validate404Response(ZumoTest test, MobileServiceInvalidOperationException msioe)
        {
            test.AddLog("Received expected exception - {0}: {1}", msioe.GetType().FullName, msioe.Message);
            var response = msioe.Response;
            if (response.StatusCode == 404)
            {
                test.AddLog("And error code is the expected one.");
                return true;
            }
            else
            {
                test.AddLog("Received error code is not the expected one: {0} - {1}", response.StatusCode, response.StatusDescription);
                return false;
            }
        }

        private static ZumoTest CreateTypedUpdateTest(
            string testName, RoundTripTableItem itemToInsert, RoundTripTableItem itemToUpdate)
        {
            return CreateTypedUpdateTest<ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate);
        }

        private static ZumoTest CreateTypedUpdateTest<TExpectedException>(
            string testName, RoundTripTableItem itemToInsert, RoundTripTableItem itemToUpdate, bool setUpdatedId = true)
            where TExpectedException : Exception
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<RoundTripTableItem>();
                try
                {
                    await table.InsertAsync(itemToInsert);
                    test.AddLog("Inserted item with id {0}", itemToInsert.Id);

                    if (setUpdatedId)
                    {
                        itemToUpdate.Id = itemToInsert.Id;
                    }

                    var expectedItem = itemToUpdate.Clone();

                    await table.UpdateAsync(itemToUpdate);
                    test.AddLog("Updated item; now retrieving it to compare with the expected value");

                    var retrievedItem = await table.LookupAsync(itemToInsert.Id);
                    test.AddLog("Retrieved item");

                    if (!expectedItem.Equals(retrievedItem))
                    {
                        test.AddLog("Error, retrieved item is different than the expected value. Expected: {0}; actual: {1}", expectedItem, retrievedItem);
                        return false;
                    }

                    // cleanup
                    await table.DeleteAsync(retrievedItem);

                    if (typeof(TExpectedException) == typeof(ExceptionTypeWhichWillNeverBeThrown))
                    {
                        return true;
                    }
                    else
                    {
                        test.AddLog("Error, test should have failed with {0}, but succeeded.", typeof(TExpectedException).FullName);
                        return false;
                    }
                }
                catch (TExpectedException ex)
                {
                    test.AddLog("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                    return true;
                }
            });
        }

        private static ZumoTest CreateUntypedUpdateTest(
            string testName, JsonObject itemToInsert, JsonObject itemToUpdate)
        {
            return CreateUntypedUpdateTest<ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate);
        }

        private static ZumoTest CreateUntypedUpdateTest<TExpectedException>(
            string testName, JsonObject itemToInsert, JsonObject itemToUpdate, bool setUpdatedId = true)
            where TExpectedException : Exception
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(ZumoTestGlobals.RoundTripTableName);
                try
                {
                    await table.InsertAsync(itemToInsert);
                    int id = (int)itemToInsert["id"].GetNumber();
                    test.AddLog("Inserted item with id {0}", id);

                    if (setUpdatedId)
                    {
                        itemToUpdate["id"] = JsonValue.CreateNumberValue(id);
                    }

                    var expectedItem = JsonObject.Parse(itemToUpdate.Stringify());

                    await table.UpdateAsync(itemToUpdate);
                    test.AddLog("Updated item; now retrieving it to compare with the expected value");

                    var retrievedItem = await table.LookupAsync(id);
                    test.AddLog("Retrieved item");

                    List<string> errors = new List<string>();
                    if (!Util.CompareJson(expectedItem, retrievedItem, errors))
                    {
                        foreach (var error in errors)
                        {
                            test.AddLog(error);
                        }

                        test.AddLog("Error, retrieved item is different than the expected value. Expected: {0}; actual: {1}", expectedItem.Stringify(), retrievedItem.Stringify());
                        return false;
                    }

                    // cleanup
                    await table.DeleteAsync(JsonObject.Parse("{\"id\":" + id + "}"));

                    if (typeof(TExpectedException) == typeof(ExceptionTypeWhichWillNeverBeThrown))
                    {
                        return true;
                    }
                    else
                    {
                        test.AddLog("Error, test should have failed with {0}, but succeeded.", typeof(TExpectedException).FullName);
                        return false;
                    }
                }
                catch (TExpectedException ex)
                {
                    test.AddLog("Caught expected exception - {0}: {1}", ex.GetType().FullName, ex.Message);
                    return true;
                }
            });
        }
    }
}
