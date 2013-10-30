// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------    

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Microsoft.WindowsAzure.MobileServices.TestFramework;
using Newtonsoft.Json;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    public class Product
    {
        public long Id { get; set; }
        public int SmallId { get; set; }
        public ulong UnsignedId { get; set; }
        public uint UnsignedSmallId { get; set; }
        public string Name { get; set; }

        [JsonProperty(Required = Required.Always)]
        public float Weight { get; set; }

        [JsonProperty(Required = Required.AllowNull)]
        public float? WeightInKG { get; set; }

        public Decimal Price { get; set; }
        public bool InStock { get; set; }
        public short DisplayAisle { get; set; }
        public ushort UnsignedDisplayAisle { get; set; }
        public byte OptionFlags { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public TimeSpan AvailableTime { get; set; }
        public List<string> Tags { get; set; }
        public ProductType Type { get; set; }

        public Product()
        {
        }

        public Product(long id)
        {
            this.Id = id;
        }
    }

    public enum ProductType
    {
        Food,
        Furniture,
    }

    [Tag("query")]
    [Tag("unit")]
    public class ZumoQuery : TestBase
    {
        private MobileServiceTableQueryDescription Compile<T, U>(Func<IMobileServiceTable<T>, IMobileServiceTableQuery<U>> getQuery)
        {
            IMobileServiceClient service = new MobileServiceClient("http://www.test.com");
            IMobileServiceTable<T> table = service.GetTable<T>();
            IMobileServiceTableQuery<U> query = getQuery(table);
            MobileServiceTableQueryProvider provider = new MobileServiceTableQueryProvider();
            MobileServiceTableQueryDescription compiledQuery = provider.Compile((MobileServiceTableQuery<U>)query);
            Log(">>> " + compiledQuery.ToQueryString());
            return compiledQuery;
        }

        [TestMethod]
        public void BasicQuery()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                select p);
            Assert.AreEqual("Product", query.TableName);
            Assert.IsNull(query.Filter);
            Assert.AreEqual(2, query.Selection.Count);
            Assert.AreEqual(0, query.Ordering.Count);
        }

        [TestMethod]
        public void Ordering()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                orderby p.Price ascending
                select p);
            Assert.AreEqual(1, query.Ordering.Count);
            Assert.AreEqual("Price", query.Ordering[0].Key);
            Assert.IsTrue(query.Ordering[0].Value);

            // Chaining
            query = Compile<Product, Product>(table => table.OrderBy(p => p.Price));
            Assert.AreEqual(1, query.Ordering.Count);
            Assert.AreEqual("Price", query.Ordering[0].Key);
            Assert.IsTrue(query.Ordering[0].Value);

            // Query syntax descending
            query = Compile<Product, Product>(table =>
                from p in table
                orderby p.Price descending
                select p);
            Assert.AreEqual(1, query.Ordering.Count);
            Assert.AreEqual("Price", query.Ordering[0].Key);
            Assert.IsFalse(query.Ordering[0].Value);

            // Chaining descending
            query = Compile<Product, Product>(table => table.OrderByDescending(p => p.Price));
            Assert.AreEqual(1, query.Ordering.Count);
            Assert.AreEqual("Price", query.Ordering[0].Key);
            Assert.IsFalse(query.Ordering[0].Value);

            // Query syntax with multiple
            query = Compile<Product, Product>(table =>
                from p in table
                orderby p.Price ascending, p.Name descending
                select p);
            Assert.AreEqual(2, query.Ordering.Count);
            Assert.AreEqual("Price", query.Ordering[0].Key);
            Assert.IsTrue(query.Ordering[0].Value);
            Assert.AreEqual("Name", query.Ordering[1].Key);
            Assert.IsFalse(query.Ordering[1].Value);

            // Chaining with multiple
            query = Compile<Product, Product>(table =>
                table
                .OrderBy(p => p.Price)
                .ThenByDescending(p => p.Name));
            Assert.AreEqual(2, query.Ordering.Count);
            Assert.AreEqual("Price", query.Ordering[0].Key);
            Assert.IsTrue(query.Ordering[0].Value);
            Assert.AreEqual("Name", query.Ordering[1].Key);
            Assert.IsFalse(query.Ordering[1].Value);
        }

        [TestMethod]
        public void Projection()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, string>(table =>
                from p in table
                select p.Name);
            Assert.AreEqual(3, query.Selection.Count);
            Assert.AreEqual("Name", query.Selection[0]);
            Assert.AreEqual("Weight", query.Selection[1]);
            Assert.AreEqual("WeightInKG", query.Selection[2]);
            Assert.AreEqual(typeof(Product), query.ProjectionArgumentType);
            Assert.AreEqual(
                "ZUMO",
                query.Projections.First().DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true }));

            // Chaining
            query = Compile<Product, string>(table => table.Select(p => p.Name));
            Assert.AreEqual(3, query.Selection.Count);
            Assert.AreEqual("Name", query.Selection[0]);
            Assert.AreEqual("Weight", query.Selection[1]);
            Assert.AreEqual("WeightInKG", query.Selection[2]);
            Assert.AreEqual(typeof(Product), query.ProjectionArgumentType);
            Assert.AreEqual(
                "ZUMO",
                query.Projections.First().DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true }));

            // Verify that we don't blow up by trying to include the Foo
            // property in the compiled query
            Compile((IMobileServiceTable<Product> table) =>
                from p in table
                select new { Foo = p.Name });
        }

        [TestMethod]
        public void MutlipleProjection()
        {
            // Chaining
            MobileServiceTableQueryDescription query = Compile<Product, string>(table =>
                table.Select(p => new { Foo = p.Name })
                     .Select(f => f.Foo.ToLower()));
            Assert.AreEqual(3, query.Selection.Count);
            Assert.AreEqual("Name", query.Selection[0]);
            Assert.AreEqual("Weight", query.Selection[1]);
            Assert.AreEqual("WeightInKG", query.Selection[2]);
            Assert.AreEqual(typeof(Product), query.ProjectionArgumentType);
            Assert.AreEqual(
                "zumo",
                query.Projections[1].DynamicInvoke(
                    query.Projections[0].DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true })));

            // Verify that we don't blow up by trying to include the Foo
            // property in the compiled query
            Compile((IMobileServiceTable<Product> table) =>
                table.Select(p => new { Foo = p.Name })
                     .Select(f => new { LowerFoo = f.Foo.ToLower() }));
        }

        [TestMethod]
        public void SkipTake()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(2).Take(5));
            Assert.AreEqual(2, query.Skip);
            Assert.AreEqual(5, query.Top);

            // Chaining
            query = Compile<Product, Product>(table => table.Select(p => p).Skip(2).Take(5));
            Assert.AreEqual(2, query.Skip);
            Assert.AreEqual(5, query.Top);

            // Allow New operations
            query = Compile<Product, Product>(table => table.Select(p => p).Skip(new Product() { SmallId = 2 }.SmallId).Take(5));
            Assert.AreEqual(2, query.Skip);
            Assert.AreEqual(5, query.Top);
        }

        [TestMethod]
        public void WithParameters()
        {
            var userParmeters1 = new Dictionary<string, string>() { { "state", "PA" } };
            var userParmeters2 = new Dictionary<string, string>() { { "country", "USA" } };

            IMobileServiceClient service = new MobileServiceClient("http://www.test.com");
            IMobileServiceTable<Product> t = service.GetTable<Product>();

            IMobileServiceTableQuery<Product> originalQuery = from p in t select p;
            IMobileServiceTableQuery<Product> query = originalQuery.WithParameters(userParmeters1)
                                                                   .Skip(2)
                                                                   .WithParameters(userParmeters2);

            Assert.AreEqual("PA", originalQuery.Parameters["state"], "original query should also have parameters");
            Assert.AreEqual("USA", originalQuery.Parameters["country"], "original query should also have parameters");
            Assert.AreEqual("PA", query.Parameters["state"]);
            Assert.AreEqual("USA", query.Parameters["country"]);
        }

        [TestMethod]
        public void Filtering()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Price > 50
                select p);
            Assert.AreEqual("(Price gt 50M)", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => p.Price > 50));
            Assert.AreEqual("(Price gt 50M)", query.Filter);

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight <= 10
                select p);
            Assert.AreEqual("(Weight le 10f)", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= 10));
            Assert.AreEqual("(Weight le 10f)", query.Filter);

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight <= 10 && p.InStock == true
                select p);
            Assert.AreEqual("((Weight le 10f) and (InStock eq true))", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= 10 && p.InStock == true));
            Assert.AreEqual("((Weight le 10f) and (InStock eq true))", query.Filter);

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight <= 10 || p.InStock == true
                select p);
            Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= 10 || p.InStock == true));
            Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter);

            query = Compile<Product, Product>(table =>
                from p in table
                where !p.InStock
                select p);
            Assert.AreEqual("not(InStock)", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => !p.InStock));
            Assert.AreEqual("not(InStock)", query.Filter);

            // Allow New Operations
            float foo = 10;
            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product() { Weight = foo }.Weight || p.InStock == true));
            Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product(15) { Weight = foo }.Weight || p.InStock == true));
            Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product(15) { Weight = 10 }.Weight || p.InStock == true));
            Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter);

            long id = 15;
            query = Compile<Product, Product>(table => table.Where(p => p.Weight <= new Product(id) { Weight = 10 }.Weight || p.InStock == true));
            Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => p.Created == new DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc)));
            Assert.AreEqual("(Created eq datetime'1994-10-14T00:00:00.000Z')", query.Filter);
        }

        [TestMethod]
        public void CombinedQuery()
        {
            MobileServiceTableQueryDescription query = Compile((IMobileServiceTable<Product> table) =>
                (from p in table
                 where p.Price <= 10M && p.Weight > 10f
                 where !p.InStock
                 orderby p.Price descending, p.Name
                 select new { p.Name, p.Price })
                .Skip(20)
                .Take(10));
            Assert.AreEqual(
                "$filter=((Price le 10M) and (Weight gt 10f)) and not(InStock)&$orderby=Price desc,Name&$skip=20&$top=10&$select=Name,Price,Weight,WeightInKG",
                query.ToQueryString());
        }

        [TestMethod]
        public void Bug466610UsingShorts()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                table.Where(p => p.DisplayAisle == 2));
            Assert.AreEqual(query.Filter, "(DisplayAisle eq 2)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.DisplayAisle == (short)3
                select p);
            Assert.AreEqual(query.Filter, "(DisplayAisle eq 3)");

            short closedOverVariable = (short)5;
            query = Compile<Product, Product>(table =>
                from p in table
                where p.DisplayAisle == closedOverVariable
                select p);
            Assert.AreEqual(query.Filter, "(DisplayAisle eq 5)");

            query = Compile<Product, Product>(table =>
                table.Where(p => p.OptionFlags == 7));
            Assert.AreEqual(query.Filter, "(OptionFlags eq 7)");

            // Verify non-numeric conversions still aren't supported
            object aisle = 12.0;
            Throws<NotSupportedException>(() =>
                Compile<Product, Product>(table =>
                    from p in table
                    where (object)p.DisplayAisle == aisle
                    select p));
        }

        [TestMethod]
        public void Bug466610BareSkipTake()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table => table.Skip(3));
            Assert.AreEqual(query.Skip, 3);
            Assert.IsFalse(query.Top.HasValue);

            query = Compile<Product, Product>(table => table.Take(5));
            Assert.AreEqual(query.Top, 5);
            Assert.IsFalse(query.Skip.HasValue);

            query = Compile<Product, Product>(table => table.Skip(7).Take(9));
            Assert.AreEqual(query.Skip, 7);
            Assert.AreEqual(query.Top, 9);

            query = Compile<Product, Product>(table => table.Take(11).Skip(13));
            Assert.AreEqual(query.Skip, 13);
            Assert.AreEqual(query.Top, 11);
        }

        [TestMethod]
        public void FilterOperators()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Name + "x" == "mx"
                select p);
            Assert.AreEqual(query.Filter, "(concat(Name,'x') eq 'mx')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight + 1.0 == 10.0
                select p);
            Assert.AreEqual(query.Filter, "((Weight add 1) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight - 1.0 == 10.0
                select p);
            Assert.AreEqual(query.Filter, "((Weight sub 1) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight * 2.0 == 10.0
                select p);
            Assert.AreEqual(query.Filter, "((Weight mul 2) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight / 2.0 == 10.0
                select p);
            Assert.AreEqual(query.Filter, "((Weight div 2) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Id % 2 == 1
                select p);
            Assert.AreEqual(query.Filter, "((id mod 2L) eq 1L)");

            query = Compile<Product, Product>(table =>
                from p in table
                where (p.Weight * 2.0) / 3.0 + 1.0 == 10.0
                select p);
            Assert.AreEqual(query.Filter, "((((Weight mul 2) div 3) add 1) eq 10)");
        }

        [TestMethod]
        public void FilterMethods()
        {
            // Methods that look like properties
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Length == 7
                select p);
            Assert.AreEqual(query.Filter, "(length(Name) eq 7)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Day == 7
                select p);
            Assert.AreEqual(query.Filter, "(day(Created) eq 7)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Month == 7
                select p);
            Assert.AreEqual(query.Filter, "(month(Created) eq 7)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Year == 7
                select p);
            Assert.AreEqual(query.Filter, "(year(Created) eq 7)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Hour == 7
                select p);
            Assert.AreEqual(query.Filter, "(hour(Created) eq 7)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Minute == 7
                select p);
            Assert.AreEqual(query.Filter, "(minute(Created) eq 7)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.Second == 7
                select p);
            Assert.AreEqual(query.Filter, "(second(Created) eq 7)");


            // Static methods
            query = Compile<Product, Product>(table =>
                from p in table
                where Math.Floor(p.Weight) == 10
                select p);
            Assert.AreEqual(query.Filter, "(floor(Weight) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where Decimal.Floor(p.Price) == 10
                select p);
            Assert.AreEqual(query.Filter, "(floor(Price) eq 10M)");

            query = Compile<Product, Product>(table =>
                from p in table
                where Math.Ceiling(p.Weight) == 10
                select p);
            Assert.AreEqual(query.Filter, "(ceiling(Weight) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where Decimal.Ceiling(p.Price) == 10
                select p);
            Assert.AreEqual(query.Filter, "(ceiling(Price) eq 10M)");

            query = Compile<Product, Product>(table =>
                from p in table
                where Math.Round(p.Weight) == 10
                select p);
            Assert.AreEqual(query.Filter, "(round(Weight) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where Math.Round(p.Price) == 10
                select p);
            Assert.AreEqual(query.Filter, "(round(Price) eq 10M)");

            query = Compile<Product, Product>(table =>
                from p in table
                where string.Concat(p.Name, "x") == "mx"
                select p);
            Assert.AreEqual(query.Filter, "(concat(Name,'x') eq 'mx')");

            // Instance methods
            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToLower() == "a"
                select p);
            Assert.AreEqual(query.Filter, "(tolower(Name) eq 'a')");

            // Instance methods
            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToLowerInvariant() == "a"
                select p);
            Assert.AreEqual(query.Filter, "(tolower(Name) eq 'a')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToUpper() == "A"
                select p);
            Assert.AreEqual(query.Filter, "(toupper(Name) eq 'A')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToUpperInvariant() == "A"
                select p);
            Assert.AreEqual(query.Filter, "(toupper(Name) eq 'A')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Trim() == "A"
                select p);
            Assert.AreEqual(query.Filter, "(trim(Name) eq 'A')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.StartsWith("x")
                select p);
            Assert.AreEqual(query.Filter, "startswith(Name,'x')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.EndsWith("x")
                select p);
            Assert.AreEqual(query.Filter, "endswith(Name,'x')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.IndexOf("x") == 2
                select p);
            Assert.AreEqual(query.Filter, "(indexof(Name,'x') eq 2)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.IndexOf('x') == 2
                select p);
            Assert.AreEqual(query.Filter, "(indexof(Name,'x') eq 2)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Contains("x")
                select p);
            Assert.AreEqual(query.Filter, "substringof('x',Name)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Replace("a", "A") == "A"
                select p);
            Assert.AreEqual(query.Filter, "(replace(Name,'a','A') eq 'A')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Replace('a', 'A') == "A"
                select p);
            Assert.AreEqual(query.Filter, "(replace(Name,'a','A') eq 'A')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Substring(2) == "A"
                select p);
            Assert.AreEqual(query.Filter, "(substring(Name,2) eq 'A')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.Substring(2, 3) == "A"
                select p);
            Assert.AreEqual(query.Filter, "(substring(Name,2,3) eq 'A')");

            // Verify each type works on nested expressions too
            query = Compile<Product, Product>(table =>
                from p in table
                where (p.Name + "x").Length == 7
                select p);
            Assert.AreEqual(query.Filter, "(length(concat(Name,'x')) eq 7)");

            query = Compile<Product, Product>(table =>
                from p in table
                where string.Concat(p.Name + "x", "x") == "mx"
                select p);
            Assert.AreEqual(query.Filter, "(concat(concat(Name,'x'),'x') eq 'mx')");

            query = Compile<Product, Product>(table =>
                from p in table
                where (p.Name + "x").ToLower() == "ax"
                select p);
            Assert.AreEqual(query.Filter, "(tolower(concat(Name,'x')) eq 'ax')");
        }

        [TestMethod]
        public void FilterContainsMethod()
        {
            string[] names = new[] { "name1", "name2" };
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where names.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            IEnumerable<string> namesEnum = new[] { "name1", "name2" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesEnum.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            IEnumerable<string> empty = Enumerable.Empty<string>();
            query = Compile<Product, Product>(table =>
                from p in table
                where empty.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, string.Empty);

            //test Contains on List<T>
            List<string> namesList = new List<string>() { "name1", "name2" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");


            //test Contains on Collection<T>
            Collection<string> coll = new Collection<string>() { "name1", "name2" };
            query = Compile<Product, Product>(table =>
                from p in table
                where coll.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            //test duplicates
            namesList = new List<string>() { "name1", "name1" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "((Name eq 'name1') or (Name eq 'name1'))");

            //test single
            namesList = new List<string>() { "name1" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "(Name eq 'name1')");

            //test multiples
            namesList = new List<string>() { "name1", "name2", "name3" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "(((Name eq 'name1') or (Name eq 'name2')) or (Name eq 'name3'))");


            IEnumerable<string> productNames = new Product[] { new Product { Name = "name1" }, new Product { Name = "name2" } }.Select(pr => pr.Name);
            query = Compile<Product, Product>(table =>
                from p in table
                where productNames.Contains(p.Name)
                select p);
            Assert.AreEqual(query.Filter, "((Name eq 'name1') or (Name eq 'name2'))");

            IEnumerable<bool> booleans = new[] { false, true };
            query = Compile<Product, Product>(table =>
                from p in table
                where booleans.Contains(p.InStock)
                select p);
            Assert.AreEqual(query.Filter, "((InStock eq false) or (InStock eq true))");

            IEnumerable<int> numbers = new[] { 13, 6 };
            query = Compile<Product, Product>(table =>
                from p in table
                where numbers.Contains(p.DisplayAisle)
                select p);
            Assert.AreEqual(query.Filter, "((DisplayAisle eq 13) or (DisplayAisle eq 6))");

            IEnumerable<double> doubles = new[] { 4.6, 3.9089 };
            query = Compile<Product, Product>(table =>
                from p in table
                where doubles.Contains(p.Weight)
                select p);
            Assert.AreEqual(query.Filter, "((Weight eq 4.6) or (Weight eq 3.9089))");
        }

        [TestMethod]
        public void FilterContainsMethodNegative()
        {
            //Contains on parameter members is not supported
            Throws<NotSupportedException>(() =>
                Compile<Product, Product>(table =>
                    from p in table
                    where p.Tags.Contains(p.Name)
                    select p));

            //test Select inside Where
            IEnumerable<Product> products = new Product[] { new Product { Name = "name1" }, new Product { Name = "name2" } };
            Throws<NotSupportedException>(() =>
                Compile<Product, Product>(table =>
                    from p in table
                    where products.Select(pr => pr.Name).Contains(p.Name)
                    select p));

            IEnumerable<int> numbers = new[] { 13, 6, 5 };
            //multiple parameter expressions throw in Contains
            Throws<NotSupportedException>(() =>
                Compile<Product, Product>(table =>
                    from p in table
                    where numbers.Select(n => p.Id).Contains(p.DisplayAisle)
                    select p));

            IEnumerable<object> objects = new object[] { 4.6, "string" };
            //Contains on list of T where T != typeof(p.Member)
            Throws<InvalidOperationException>(() =>
                Compile<Product, Product>(table =>
                    from p in table
                    where objects.Contains(p.Weight)
                    select p));
        }

        [TestMethod]
        public void FilterEnums()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Type == ProductType.Food
                select p);
            Assert.AreEqual(query.Filter, "(Type eq 'Food')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum1 == Enum1.Enum1Value2
                select p);
            Assert.AreEqual(query.Filter, "(Enum1 eq 'Enum1Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum2 == Enum2.Enum2Value2
                select p);
            Assert.AreEqual(query.Filter, "(Enum2 eq 'Enum2Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum3 == (Enum3.Enum3Value2 | Enum3.Enum3Value1)
                select p);
            Assert.AreEqual(query.Filter, "(Enum3 eq 'Enum3Value1%2C%20Enum3Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum4 == Enum4.Enum4Value2
                select p);
            Assert.AreEqual(query.Filter, "(Enum4 eq 'Enum4Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum5 == Enum5.Enum5Value2
                select p);
            Assert.AreEqual(query.Filter, "(Enum5 eq 'Enum5Value2')");

            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum6 == Enum6.Enum6Value1
                select p);
            Assert.AreEqual(query.Filter, "(Enum6 eq 'Enum6Value1')");

            //check if toString works
            query = Compile<EnumType, EnumType>(table =>
                from p in table
                where p.Enum6.ToString() == Enum6.Enum6Value1.ToString()
                select p);
            Assert.AreEqual(query.Filter, "(Enum6 eq 'Enum6Value1')");
        }

        [TestMethod]
        public void ToStringTest()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Tags.ToString() == "Test"
                select p);
            Assert.AreEqual(query.Filter, "(Tags eq 'Test')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Price.ToString() == "1.56"
                select p);
            Assert.AreEqual(query.Filter, "(Price eq '1.56')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Created.ToString() == "January 23"
                select p);
            Assert.AreEqual(query.Filter, "(Created eq 'January%2023')");

            IList<string> namesList = new List<string>() { "name1", "name1" };
            query = Compile<Product, Product>(table =>
                from p in table
                where namesList.Contains(p.DisplayAisle.ToString())
                select p);
            Assert.AreEqual(query.Filter, "((DisplayAisle eq 'name1') or (DisplayAisle eq 'name1'))");
        }

        [TestMethod]
        public void FilterReturnsTrueConstant()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where true
                select p);
            Assert.AreEqual(query.Filter, "true");

            query = Compile<Product, Product>(table => table.Where(p => true));
            Assert.AreEqual(query.Filter, "true");

            query = Compile<Product, Product>(table => table.Where(item => !item.InStock || item.InStock));
            Assert.AreEqual(query.Filter, "(not(InStock) or InStock)");
        }

        [TestMethod]
        public void FilterNullable()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Updated == null
                select p);
            Assert.AreEqual(query.Filter, "(Updated eq null)");

            query = Compile<Product, Product>(table =>
               from p in table
               where p.Updated == DateTime.MinValue
               select p);
            Assert.AreEqual(query.Filter, "(Updated eq datetime'0001-01-01T08:00:00.000Z')");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.WeightInKG == null
                select p);
            Assert.AreEqual(query.Filter, "(WeightInKG eq null)");

            query = Compile<Product, Product>(table =>
                from p in table
                where p.WeightInKG == 4.0
                select p);
            Assert.AreEqual(query.Filter, "(WeightInKG eq 4)");
        }

        [TestMethod]
        public void MultipleSkipShouldAddUp()
        {
            // Query syntax
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(2).Skip(5));
            Assert.AreEqual(7, query.Skip);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(5).Skip(5));
            Assert.AreEqual(10, query.Skip);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Skip(20).Skip(6).Skip(5).Skip(2).Skip(9).Skip(5));
            Assert.AreEqual(47, query.Skip);
        }

        [TestMethod]
        public void MultipleTakeShouldChooseSmallest()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Take(2).Take(5));
            Assert.AreEqual(2, query.Top);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Take(5).Take(2));
            Assert.AreEqual(2, query.Top);

            query = Compile<Product, Product>(table =>
                (from p in table
                 select p).Take(5).Take(5));
            Assert.AreEqual(5, query.Top);
        }

        [TestMethod]
        public void TableNameShouldPropagateCorrectly()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                select p);
            Assert.AreEqual("Product", query.TableName);
            Assert.IsNull(query.Filter);
            Assert.AreEqual(2, query.Selection.Count);
            Assert.AreEqual(0, query.Ordering.Count);

            query = Compile<Product, string>(table =>
                from p in table
                select p.Name);
            Assert.AreEqual("Product", query.TableName);
            Assert.IsNull(query.Filter);
            Assert.AreEqual(3, query.Selection.Count);
            Assert.AreEqual(0, query.Ordering.Count);
        }

        [TestMethod]
        public void BooleanMemberValuesShouldBeHandled()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.InStock
                select p);
            Assert.AreEqual("Product", query.TableName);
            Assert.AreEqual("InStock", query.Filter);
            Assert.AreEqual(0, query.Selection.Count);
            Assert.AreEqual(0, query.Ordering.Count);
        }

        [TestMethod]
        public void UnsignedDataTypesTest()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.UnsignedId == 12UL
                select p);
            Assert.AreEqual("Product", query.TableName);
            Assert.AreEqual("(UnsignedId eq 12L)", query.Filter);

            //unsigned ints should be sent as long
            query = Compile<Product, Product>(table =>
                from p in table
                where p.UnsignedSmallId == 12U //uint
                select p);
            Assert.AreEqual("Product", query.TableName);
            Assert.AreEqual("(UnsignedSmallId eq 12L)", query.Filter);

            query = Compile<Product, Product>(table =>
                from p in table
                where p.UnsignedDisplayAisle == (ushort)12
                select p);
            Assert.AreEqual("Product", query.TableName);
            Assert.AreEqual("(UnsignedDisplayAisle eq 12)", query.Filter);
        }

        [TestMethod]
        public void FilterBasedOnTimeSpanShouldResultInString()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.AvailableTime > TimeSpan.FromDays(1)
                select p);
            Assert.AreEqual("Product", query.TableName);
            Assert.AreEqual("(AvailableTime gt '1.00%3A00%3A00')", query.Filter);
            Assert.AreEqual(0, query.Selection.Count);
            Assert.AreEqual(0, query.Ordering.Count);
        }

        [TestMethod]
        public void SkipZero()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table => table.Skip(0));
            Assert.AreEqual("Product", query.TableName);
            Assert.AreEqual(0, query.Skip);
            Assert.IsFalse(query.Top.HasValue);
            Assert.AreEqual("$skip=0", query.ToQueryString());
        }

        [TestMethod]
        public void TopZero()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table => table.Take(0));
            Assert.AreEqual("Product", query.TableName);
            Assert.AreEqual(0, query.Top);
            Assert.IsFalse(query.Skip.HasValue);
            Assert.AreEqual("$top=0", query.ToQueryString());
        }

        [TestMethod]
        public void DoublesSerializedUsingInvariantCulture()
        {
            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                from p in table
                where p.Weight > 1.3f
                select p);
            Assert.AreEqual("(Weight gt 1.3f)", query.Filter);
        }
    }
}
