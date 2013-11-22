// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            result.AddTest(CreateTypedUpdateTest<RoundTripTableItem, MobileServiceInvalidOperationException>("(Neg) Update typed item, non-existing item id",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = 1000000000 }, false));
            result.AddTest(CreateTypedUpdateTest<RoundTripTableItem, ArgumentException>("(Neg) Update typed item, id = 0",
                new RoundTripTableItem(rndGen), new RoundTripTableItem(rndGen) { Id = 0 }, false));

            result.AddTest(CreateTypedUpdateTest("[string id] Update typed item",
                new StringIdRoundTripTableItem(rndGen), new StringIdRoundTripTableItem(rndGen)));
            result.AddTest(CreateTypedUpdateTest(
                "[string id] Update typed item, setting values to null",
                new StringIdRoundTripTableItem(rndGen),
                new StringIdRoundTripTableItem(rndGen) { Name = null, Bool = null, ComplexType = null, Date = null }));
            result.AddTest(CreateTypedUpdateTest<StringIdRoundTripTableItem, MobileServiceInvalidOperationException>("(Neg) Update typed item, non-existing item id",
                new StringIdRoundTripTableItem(rndGen), new StringIdRoundTripTableItem(rndGen) { Id = "does not exist" }, false));
            result.AddTest(CreateTypedUpdateTest<StringIdRoundTripTableItem, ArgumentException>("(Neg) Update typed item, id = null",
                new StringIdRoundTripTableItem(rndGen), new StringIdRoundTripTableItem(rndGen) { Id = null }, false));

            string toInsertJsonString = @"{
                ""string1"":""hello"",
                ""bool1"":true,
                ""int1"":-1234,
                ""double1"":123.45,
                ""long1"":1234,
                ""date1"":""2012-12-13T09:23:12.000Z"",
                ""complexType1"":[{""Name"":""John Doe"",""Age"":33}, null],
                ""complexType2"":{""Name"":""John Doe"",""Age"":33,""Friends"":[""Jane""]}
            }";

            string toUpdateJsonString = @"{
                ""string1"":""world"",
                ""bool1"":false,
                ""int1"":9999,
                ""double1"":888.88,
                ""long1"":77777777,
                ""date1"":""1999-05-23T19:15:54.000Z"",
                ""complexType1"":[{""Name"":""Jane Roe"",""Age"":23}, null],
                ""complexType2"":{""Name"":""Jane Roe"",""Age"":23,""Friends"":[""John""]}
            }";

            result.AddTest(CreateUntypedUpdateTest("Update untyped item", toInsertJsonString, toUpdateJsonString));

            JToken nullValue = JValue.Parse("null");
            JObject toUpdate = JObject.Parse(toUpdateJsonString);
            toUpdate["string1"] = nullValue;
            toUpdate["bool1"] = nullValue;
            toUpdate["complexType2"] = nullValue;
            toUpdate["complexType1"] = nullValue;
            toUpdate["int1"] = nullValue;
            result.AddTest(CreateUntypedUpdateTest("Update untyped item, setting values to null", toInsertJsonString, toUpdate.ToString()));

            toUpdate["id"] = 1000000000;
            result.AddTest(CreateUntypedUpdateTest<MobileServiceInvalidOperationException>("(Neg) Update untyped item, non-existing item id",
                toInsertJsonString, toUpdate.ToString(), false));

            toUpdate["id"] = 0;
            result.AddTest(CreateUntypedUpdateTest<ArgumentException>("(Neg) Update typed item, id = 0",
                toInsertJsonString, toUpdateJsonString, false));

            toInsertJsonString = JsonConvert.SerializeObject(new StringIdRoundTripTableItem(rndGen) { Id = null });
            toUpdateJsonString = JsonConvert.SerializeObject(new StringIdRoundTripTableItem(rndGen) { Id = null });
            result.AddTest(CreateUntypedUpdateTest("[string id] Update untyped item", toInsertJsonString, toUpdateJsonString, true));
            toUpdate = JObject.Parse(toUpdateJsonString);
            toUpdate["name"] = nullValue;
            toUpdate["number"] = nullValue;
            toUpdate["date1"] = nullValue;
            toUpdate["bool"] = nullValue;
            toUpdate["complex"] = nullValue;
            result.AddTest(CreateUntypedUpdateTest("[string id] Update untyped item, setting values to null", toInsertJsonString, toUpdate.ToString(), true));

            toUpdate["id"] = Guid.NewGuid().ToString();
            result.AddTest(CreateUntypedUpdateTest<MobileServiceInvalidOperationException>("(Neg) [string id] Update untyped item, non-existing item id",
                toInsertJsonString, toUpdate.ToString(), false, true));

            toUpdate["id"] = nullValue;
            result.AddTest(CreateUntypedUpdateTest<InvalidOperationException>("(Neg) [string id] Update typed item, id = null",
                toInsertJsonString, toUpdateJsonString, false, true));

            // Delete tests
            result.AddTest(CreateDeleteTest<RoundTripTableItem>("Delete typed item", true, DeleteTestType.ValidDelete));
            result.AddTest(CreateDeleteTest<RoundTripTableItem>("(Neg) Delete typed item with non-existing id", true, DeleteTestType.NonExistingId));
            result.AddTest(CreateDeleteTest<RoundTripTableItem>("Delete untyped item", false, DeleteTestType.ValidDelete));
            result.AddTest(CreateDeleteTest<RoundTripTableItem>("(Neg) Delete untyped item with non-existing id", false, DeleteTestType.NonExistingId));
            result.AddTest(CreateDeleteTest<RoundTripTableItem>("(Neg) Delete untyped item without id field", false, DeleteTestType.NoIdField));

            result.AddTest(CreateDeleteTest<StringIdRoundTripTableItem>("[string id] Delete typed item", true, DeleteTestType.ValidDelete));
            result.AddTest(CreateDeleteTest<StringIdRoundTripTableItem>("(Neg) [string id] Delete typed item with non-existing id", true, DeleteTestType.NonExistingId));
            result.AddTest(CreateDeleteTest<StringIdRoundTripTableItem>("[string id] Delete untyped item", false, DeleteTestType.ValidDelete));
            result.AddTest(CreateDeleteTest<StringIdRoundTripTableItem>("(Neg) [string id] Delete untyped item with non-existing id", false, DeleteTestType.NonExistingId));
            result.AddTest(CreateDeleteTest<StringIdRoundTripTableItem>("(Neg) [string id] Delete untyped item without id field", false, DeleteTestType.NoIdField));

            result.AddTest(new ZumoTest("Refresh - updating item with server modifications", async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<RoundTripTableItem>();
                int randomSeed = Environment.TickCount;
                test.AddLog("Using random seed {0}", randomSeed);
                Random random = new Random(randomSeed);
                var item = new RoundTripTableItem(random);
                test.AddLog("Item to be inserted: {0}", item);
                await table.InsertAsync(item);
                test.AddLog("Added item with id = {0}", item.Id);

                var item2 = new RoundTripTableItem(random);
                item2.Id = item.Id;
                test.AddLog("Item to update: {0}", item2);
                await table.UpdateAsync(item2);
                test.AddLog("Updated item");

                test.AddLog("Now refreshing first object");
                await table.RefreshAsync(item);
                test.AddLog("Refreshed item: {0}", item);

                if (item.Equals(item2))
                {
                    test.AddLog("Item was refreshed successfully");
                    return true;
                }
                else
                {
                    test.AddLog("Error, refresh didn't happen successfully");
                    return false;
                }
            }));

            result.AddTest(new ZumoTest("Refresh item without id does not send request", async delegate(ZumoTest test)
            {
                var client = new MobileServiceClient(
                    ZumoTestGlobals.Instance.Client.ApplicationUri,
                    ZumoTestGlobals.Instance.Client.ApplicationKey,
                    new HandlerWhichThrows());
                var table = client.GetTable<RoundTripTableItem>();
                var item = new RoundTripTableItem(new Random());
                item.Id = 0;
                await table.RefreshAsync(item);
                test.AddLog("Call to RefreshAsync didn't go to the network, as expected.");
                return true;
            }));

            return result;
        }

        class HandlerWhichThrows : DelegatingHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        enum DeleteTestType { ValidDelete, NonExistingId, NoIdField }

        private static ZumoTest CreateDeleteTest<TItemType>(string testName, bool useTypedTable, DeleteTestType testType) where TItemType : ICloneableItem<TItemType>
        {
            if (useTypedTable && testType == DeleteTestType.NoIdField)
            {
                throw new ArgumentException("Cannot send a delete request without an id field on a typed table.");
            }

            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var typedTable = client.GetTable<TItemType>();
                var useStringIdTable = typeof(TItemType) == typeof(StringIdRoundTripTableItem);
                var untypedTable = client.GetTable(
                    useStringIdTable ? 
                    ZumoTestGlobals.StringIdRoundTripTableName : 
                    ZumoTestGlobals.RoundTripTableName);
                TItemType itemToInsert;
                if (useStringIdTable)
                {
                    itemToInsert = (TItemType)(object)new StringIdRoundTripTableItem { Name = "will be deleted", Number = 123 };
                }
                else
                {
                    itemToInsert = (TItemType)(object)new RoundTripTableItem { String1 = "will be deleted", Int1 = 123 };
                }

                await typedTable.InsertAsync(itemToInsert);
                test.AddLog("Inserted item to be deleted");
                object id = itemToInsert.Id;
                switch (testType)
                {
                    case DeleteTestType.ValidDelete:
                        if (useTypedTable)
                        {
                            await typedTable.DeleteAsync(itemToInsert);
                        }
                        else
                        {
                            await untypedTable.DeleteAsync(new JObject(new JProperty("id", id)));
                        }

                        test.AddLog("Delete succeeded; verifying that object isn't in the service anymore.");
                        try
                        {
                            var response = await untypedTable.LookupAsync(id);
                            test.AddLog("Error, delete succeeded, but item was returned by the service: {0}", response);
                            return false;
                        }
                        catch (MobileServiceInvalidOperationException msioe)
                        {
                            return Validate404Response(test, msioe);
                        }

                    case DeleteTestType.NonExistingId:
                        try
                        {
                            object nonExistingId = useStringIdTable ? (object)Guid.NewGuid().ToString() : (object)1000000000;
                            if (useTypedTable)
                            {
                                itemToInsert.Id = nonExistingId;
                                await typedTable.DeleteAsync(itemToInsert);
                            }
                            else
                            {
                                JObject jo = new JObject(new JProperty("id", nonExistingId));
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
                            JObject jo = new JObject(new JProperty("Name", "hello"));
                            await untypedTable.DeleteAsync(jo);

                            test.AddLog("Error, deleting item without an id should fail, but succeeded");
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
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                test.AddLog("And error code is the expected one.");
                return true;
            }
            else
            {
                test.AddLog("Received error code is not the expected one: {0} - {1}", response.StatusCode, response.ReasonPhrase);
                return false;
            }
        }

        private static ZumoTest CreateTypedUpdateTest<TRoundTripType>(
            string testName, TRoundTripType itemToInsert, TRoundTripType itemToUpdate) where TRoundTripType : ICloneableItem<TRoundTripType>
        {
            return CreateTypedUpdateTest<TRoundTripType, ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate);
        }

        private static ZumoTest CreateTypedUpdateTest<TRoundTripType, TExpectedException>(
            string testName, TRoundTripType itemToInsert, TRoundTripType itemToUpdate, bool setUpdatedId = true)
            where TExpectedException : Exception
            where TRoundTripType : ICloneableItem<TRoundTripType>
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable<TRoundTripType>();
                var toInsert = itemToInsert.Clone();
                var toUpdate = itemToUpdate.Clone();
                try
                {
                    await table.InsertAsync(toInsert);
                    test.AddLog("Inserted item with id {0}", toInsert.Id);

                    if (setUpdatedId)
                    {
                        toUpdate.Id = toInsert.Id;
                    }

                    var expectedItem = toUpdate.Clone();

                    await table.UpdateAsync(toUpdate);
                    test.AddLog("Updated item; now retrieving it to compare with the expected value");

                    var retrievedItem = await table.LookupAsync(toInsert.Id);
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
            string testName, string itemToInsert, string itemToUpdate, bool useStringIdTable = false)
        {
            return CreateUntypedUpdateTest<ExceptionTypeWhichWillNeverBeThrown>(testName, itemToInsert, itemToUpdate, true, useStringIdTable);
        }

        private static ZumoTest CreateUntypedUpdateTest<TExpectedException>(
            string testName, string itemToInsertJson, string itemToUpdateJson, bool setUpdatedId = true, bool useStringIdTable = false)
            where TExpectedException : Exception
        {
            return new ZumoTest(testName, async delegate(ZumoTest test)
            {
                var itemToInsert = JObject.Parse(itemToInsertJson);
                var itemToUpdate = JObject.Parse(itemToUpdateJson);
                var client = ZumoTestGlobals.Instance.Client;
                var table = client.GetTable(useStringIdTable ? ZumoTestGlobals.StringIdRoundTripTableName : ZumoTestGlobals.RoundTripTableName);
                try
                {
                    var inserted = await table.InsertAsync(itemToInsert);
                    object id = useStringIdTable ?
                        (object)(string)inserted["id"] : 
                        (object)(int)inserted["id"];
                    test.AddLog("Inserted item with id {0}", id);

                    if (setUpdatedId)
                    {
                        itemToUpdate["id"] = new JValue(id);
                    }

                    var expectedItem = JObject.Parse(itemToUpdate.ToString());

                    var updated = await table.UpdateAsync(itemToUpdate);
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

                        test.AddLog("Error, retrieved item is different than the expected value. Expected: {0}; actual: {1}", expectedItem, retrievedItem);
                        return false;
                    }

                    // cleanup
                    await table.DeleteAsync(new JObject(new JProperty("id", id)));

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
