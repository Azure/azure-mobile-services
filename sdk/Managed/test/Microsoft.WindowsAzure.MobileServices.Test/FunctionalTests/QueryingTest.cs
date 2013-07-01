// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices.TestFramework;

namespace Microsoft.WindowsAzure.MobileServices.Test
{
    /// <summary>
    /// Run most of the query unit tests against the live server to make sure
    /// none of them error and all of our OData is valid.
    /// </summary>
    [Tag("query")]
    [Tag("e2e")]
    public class QueryingTest : FunctionalTestBase
    {
        private async Task Query<T, U>(Func<IMobileServiceTable<T>, IMobileServiceTableQuery<U>> getQuery)
        {
            IMobileServiceTable<T> table = GetClient().GetTable<T>();
            IMobileServiceTableQuery<U> query = getQuery(table);
            await table.ReadAsync(query);
        }

        [AsyncTestMethod]
        public async Task LiveBasicQuery()
        {
            // Query syntax
            await Query<Book, Book>(table =>
                from p in table
                select p);
        }

        [AsyncTestMethod]
        public async Task LiveOrdering()
        {
            // Query syntax
            await Query<Book, Book>(table =>
                from p in table
                orderby p.Price ascending
                select p);

            // Chaining
            await Query<Book, Book>(table => table.OrderBy(p => p.Price));

            // Query syntax descending
            await Query<Book, Book>(table =>
                from p in table
                orderby p.Price descending
                select p);

            // Chaining descending
            await Query<Book, Book>(table => table.OrderByDescending(p => p.Price));

            // Query syntax with multiple
            await Query<Book, Book>(table =>
                from p in table
                orderby p.Price ascending
                orderby p.Title descending
                select p);

            // Chaining with multiple
            await Query<Book, Book>(table =>
                table
                .OrderBy(p => p.Price)
                .OrderByDescending(p => p.Title));
        }

        [AsyncTestMethod]
        public async Task LiveProjection()
        {
            // Query syntax
            await Query<Book, string>(table =>
                from p in table
                select p.Title);

            // Chaining
            await Query<Book, string>(table => table.Select(p => p.Title));

            // Chaining
            await Query<Book, Book>(table => table.Select(p => new { x = p.Title })
                                                  .Select(p => new Book() { Title = p.x }));

            // Verify that we don't blow up by trying to include the Foo
            // property in the compiled query
            await Query((IMobileServiceTable<Book> table) =>
                from p in table
                select new { Foo = p.Title });
        }

        [AsyncTestMethod]
        public async Task LiveSkipTake()
        {
            // Query syntax
            await Query<Book, Book>(table =>
                (from p in table
                 select p).Skip(2).Take(5));

            // Chaining
            await Query<Book, Book>(table => table.Select(p => p).Skip(2).Take(5));
        }

        [AsyncTestMethod]
        public async Task LiveFiltering()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Price > 50
                select p);

            await Query<Book, Book>(table => table.Where(p => p.Price > 50));

            await Query<Book, Book>(table =>
                from p in table
                where p.Advance <= 10
                select p);

            await Query<Book, Book>(table => table.Where(p => p.Advance <= 10));

            await Query<Book, Book>(table =>
                from p in table
                where p.Advance <= 10 && p.Id > 0
                select p);

            await Query<Book, Book>(table => table.Where(p => p.Advance <= 10 && p.Id > 0));

            await Query<Book, Book>(table =>
                from p in table
                where p.Advance <= 10 || p.Id > 0
                select p);

            await Query<Book, Book>(table => table.Where(p => p.Advance <= 10 || p.Id > 0));

            await Query<Book, Book>(table =>
                from p in table
                where !(p.Id > 0)
                select p);

            await Query<Book, Book>(table => table.Where(p => !(p.Id > 0)));

            await Query<Book, Book>(table => table.Where(p => (p.Title == "How do I dial this # & such 'things'?")));
        }

        [AsyncTestMethod]
        public async Task LiveCombinedQuery()
        {
            await Query((IMobileServiceTable<Book> table) =>
                (from p in table
                 where p.Price <= 10 && p.Advance > 10f
                 where !(p.Id > 0)
                 orderby p.Price descending
                 orderby p.Title
                 select new { p.Title, p.Price })
                .Skip(20)
                .Take(10));
        }

        [AsyncTestMethod]
        public async Task LiveFilterOperators()
        {
            await Query<Book, Book>(table =>
                from p in table
                where p.Title + "x" == "mx"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Advance + 1.0 == 10.0
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Advance - 1.0 == 10.0
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Advance * 2.0 == 10.0
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Advance / 2.0 == 10.0
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Id % 2 == 1
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where (p.Advance * 2.0) / 3.0 + 1.0 == 10.0
                select p);
        }

        [AsyncTestMethod]
        public async Task LiveFilterMethods()
        {
            // Methods that look like properties
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Length == 7
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Day == 7
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Month == 7
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Year == 7
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Hour == 7
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Minute == 7
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.PublicationDate.Second == 7
                select p);

            // Static methods
            await Query<Book, Book>(table =>
                from p in table
                where Math.Floor(p.Advance) == 10
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where Math.Floor(p.Price) == 10
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where Math.Ceiling(p.Advance) == 10
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where Math.Ceiling(p.Price) == 10
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where Math.Round(p.Advance) == 10
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where Math.Round(p.Price) == 10
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where string.Concat(p.Title, "x") == "mx"
                select p);

            // Instance methods
            await Query<Book, Book>(table =>
                from p in table
                where p.Title.ToLower() == "a"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.ToUpper() == "A"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Trim() == "A"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.StartsWith("x")
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.EndsWith("x")
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Contains("x")
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.IndexOf("x") == 2
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.IndexOf('x') == 2
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Contains("x")
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Replace("a", "A") == "A"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Replace('a', 'A') == "A"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Substring(2) == "A"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where p.Title.Substring(2, 3) == "A"
                select p);

            // Verify each type works on nested expressions too
            await Query<Book, Book>(table =>
                from p in table
                where (p.Title + "x").Length == 7
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where string.Concat(p.Title + "x", "x") == "mx"
                select p);

            await Query<Book, Book>(table =>
                from p in table
                where (p.Title + "x").ToLower() == "ax"
                select p);
        }
    }
}