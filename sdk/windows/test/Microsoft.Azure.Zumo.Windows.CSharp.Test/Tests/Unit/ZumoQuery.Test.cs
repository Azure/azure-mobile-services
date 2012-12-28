// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------    

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Zumo.Win8.Test;
using Microsoft.WindowsAzure.MobileServices;

namespace Microsoft.Azure.Zumo.Win8.CSharp.Test
{
    public class Product
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public Decimal Price { get; set; }
        public float Weight { get; set; }
        public bool InStock { get; set; }
        public short DisplayAisle { get; set; }
        public byte OptionFlags { get; set; }
        public DateTime Created { get; set; }
    }

    public class ZumoQuery : TestBase
    {
        private static MobileServiceTableQueryDescription Compile<T, U>(Func<IMobileServiceTable<T>, MobileServiceTableQuery<U>> getQuery)
        {
            MobileServiceClient service = new MobileServiceClient("http://www.test.com");
            IMobileServiceTable<T> table = service.GetTable<T>();
            MobileServiceTableQuery<U> query = getQuery(table);
            MobileServiceTableQueryDescription compiledQuery = query.Compile();
            App.Harness.Log(">>> " + compiledQuery.ToString());
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
            Assert.AreEqual(0, query.Selection.Count);
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
                orderby p.Price ascending
                orderby p.Name descending
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
                .OrderByDescending(p => p.Name));
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
            Assert.AreEqual(1, query.Selection.Count);
            Assert.AreEqual("Name", query.Selection[0]);
            Assert.AreEqual(typeof(Product), query.ProjectionArgumentType);
            Assert.AreEqual(
                "ZUMO",
                query.Projection.DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true }));

            // Chaining
            query = Compile<Product, string>(table => table.Select(p => p.Name));
            Assert.AreEqual(1, query.Selection.Count);
            Assert.AreEqual("Name", query.Selection[0]);
            Assert.AreEqual(typeof(Product), query.ProjectionArgumentType);
            Assert.AreEqual(
                "ZUMO",
                query.Projection.DynamicInvoke(
                    new Product { Name = "ZUMO", Price = 0, InStock = true }));

            // Verify that we don't blow up by trying to include the Foo
            // property in the compiled query
            Compile((IMobileServiceTable<Product> table) =>
                from p in table
                select new { Foo = p.Name });
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
        }

        [TestMethod]
        public void WithParameters()
        {
            var userParmeters1 = new Dictionary<string, string>() { { "state", "PA" } };
            var userParmeters2 = new Dictionary<string, string>() { { "country", "USA" } };

            MobileServiceTableQueryDescription query = Compile<Product, Product>(table =>
                (from p in table
                 select p).WithParameters(userParmeters1).Skip(2).WithParameters(userParmeters2));

            Assert.AreEqual(2, query.Skip);
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
            Assert.AreEqual(" not(InStock)", query.Filter);

            query = Compile<Product, Product>(table => table.Where(p => !p.InStock));
            Assert.AreEqual(" not(InStock)", query.Filter);
        }

        [TestMethod]
        public void CombinedQuery()
        {
            MobileServiceTableQueryDescription query = Compile((IMobileServiceTable<Product> table) =>
                (from p in table
                 where p.Price <= 10M && p.Weight > 10f
                 where !p.InStock
                 orderby p.Price descending
                 orderby p.Name
                 select new { p.Name, p.Price })
                .Skip(20)
                .Take(10));
            Assert.AreEqual(
                "$filter=((Price le 10M) and (Weight gt 10f)) and  not(InStock)&$orderby=Price desc,Name&$skip=20&$top=10&$select=Name,Price",
                query.ToString());
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
            Assert.AreEqual(query.Top, 0);

            query = Compile<Product, Product>(table => table.Take(5));
            Assert.AreEqual(query.Skip, 0);
            Assert.AreEqual(query.Top, 5);

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
                where Math.Floor(p.Price) == 10
                select p);
            Assert.AreEqual(query.Filter, "(floor(Price) eq 10M)");

            query = Compile<Product, Product>(table =>
                from p in table
                where Math.Ceiling(p.Weight) == 10
                select p);
            Assert.AreEqual(query.Filter, "(ceiling(Weight) eq 10)");

            query = Compile<Product, Product>(table =>
                from p in table
                where Math.Ceiling(p.Price) == 10
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

            query = Compile<Product, Product>(table =>
                from p in table
                where p.Name.ToUpper() == "A"
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
    }
}
