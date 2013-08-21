Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.WindowsAzure.MobileServices
Imports Newtonsoft.Json

<TestClass()> Public Class QueryTests

    Friend Shared Function Compile(Of T, U)(ByVal getQuery As Func(Of IMobileServiceTable(Of T), IMobileServiceTableQuery(Of U))) As MobileServiceTableQueryDescription
        Dim client = New MobileServiceClient("http://www.test.com")
        Dim table As IMobileServiceTable(Of T) = client.GetTable(Of T)()
        Dim query As IMobileServiceTableQuery(Of U) = getQuery(table)
        Dim provider As MobileServiceTableQueryProvider = New MobileServiceTableQueryProvider()
        Dim compiledQuery As MobileServiceTableQueryDescription = provider.Compile(CType(query, MobileServiceTableQuery(Of U)))
        Console.WriteLine(">>> {0}", compiledQuery.ToQueryString())
        Return compiledQuery
    End Function

    <TestMethod()> Public Sub Ordering()
        ' Query syntax
        Dim query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Order By p.Price Ascending _
                                                     Select p)
        Assert.AreEqual("Product", query.TableName)
        Assert.AreEqual(1, query.Ordering.Count)
        Assert.AreEqual("Price", query.Ordering(0).Key)
        Assert.IsTrue(query.Ordering(0).Value)

        ' Chaining
        query = Compile(Of Product, Product)(Function(table) _
                                                     table.OrderBy(Function(p) p.Price))
        Assert.AreEqual(1, query.Ordering.Count)
        Assert.AreEqual("Price", query.Ordering(0).Key)
        Assert.IsTrue(query.Ordering(0).Value)

        ' Query syntax descending
        query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Order By p.Price Descending _
                                                     Select p)
        Assert.AreEqual(1, query.Ordering.Count)
        Assert.AreEqual("Price", query.Ordering(0).Key)
        Assert.IsFalse(query.Ordering(0).Value)

        ' Chaining descending
        query = Compile(Of Product, Product)(Function(table) _
                                                     table.OrderByDescending(Function(p) p.Price))
        Assert.AreEqual(1, query.Ordering.Count)
        Assert.AreEqual("Price", query.Ordering(0).Key)
        Assert.IsFalse(query.Ordering(0).Value)

        ' Query syntax with multiple
        query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Order By p.Price Ascending, p.Name Descending _
                                                     Select p)
        Assert.AreEqual(2, query.Ordering.Count)
        Assert.AreEqual("Price", query.Ordering(0).Key)
        Assert.IsTrue(query.Ordering(0).Value)
        Assert.AreEqual("Name", query.Ordering(1).Key)
        Assert.IsFalse(query.Ordering(1).Value)

        ' Chaining with multiple
        query = Compile(Of Product, Product)(Function(table) _
                                                 table.OrderBy(Function(p) p.Price).ThenByDescending(Function(p) p.Name))
        Assert.AreEqual(2, query.Ordering.Count)
        Assert.AreEqual("Price", query.Ordering(0).Key)
        Assert.IsTrue(query.Ordering(0).Value)
        Assert.AreEqual("Name", query.Ordering(1).Key)
        Assert.IsFalse(query.Ordering(1).Value)
    End Sub

    <TestMethod> Public Sub Projection()
        ' Query syntax
        Dim query = Compile(Of Product, String)(Function(table) _
                                                     From p In table _
                                                     Select p.Name)
        Assert.AreEqual(3, query.Selection.Count)
        Assert.AreEqual("Name", query.Selection(0))
        Assert.AreEqual("Weight", query.Selection(1))
        Assert.AreEqual("WeightInKG", query.Selection(2))
        Assert.AreEqual(GetType(Product), query.ProjectionArgumentType)
        Dim product = New Product() With { _
                      .Name = "ZUMO", _
                      .Price = 1, _
                      .InStock = True
                  }
        Assert.AreEqual("ZUMO", query.Projections.First().DynamicInvoke(product))

        ' Chaining
        query = Compile(Of Product, String)(Function(table) _
                                                     table.Select(Function(p) p.Name))
        Assert.AreEqual(3, query.Selection.Count)
        Assert.AreEqual("Name", query.Selection(0))
        Assert.AreEqual("Weight", query.Selection(1))
        Assert.AreEqual("WeightInKG", query.Selection(2))
        Assert.AreEqual(GetType(Product), query.ProjectionArgumentType)
        Assert.AreEqual("ZUMO", query.Projections.First().DynamicInvoke(product))
    End Sub

    <TestMethod> Public Sub MultipleProjections()
        Dim product = New Product() With { _
                      .Name = "ZUMO", _
                      .Price = 1, _
                      .InStock = True
                  }

        ' Chaining
        Dim query = Compile(Of Product, String)(Function(table) _
                                                     table.Select(Function(p) New With {.Foo = p.Name}) _
                                                     .Select(Function(f) f.Foo.ToLower()))
        Assert.AreEqual(3, query.Selection.Count)
        Assert.AreEqual("Name", query.Selection(0))
        Assert.AreEqual("Weight", query.Selection(1))
        Assert.AreEqual("WeightInKG", query.Selection(2))
        Assert.AreEqual(GetType(Product), query.ProjectionArgumentType)
        Assert.AreEqual("zumo", _
                        query.Projections(1).DynamicInvoke( _
                            query.Projections(0).DynamicInvoke(product)))
    End Sub

    <TestMethod> Public Sub SkipTake()
        Dim query = Compile(Of Product, Product)(Function(table) _
                                                     table.Skip(2).Take(5))
        Assert.AreEqual(2, query.Skip)
        Assert.AreEqual(5, query.Top)

        ' Allow new operations
        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Skip(New Product() With {.SmallId = 2}.SmallId).Take(5))
        Assert.AreEqual(2, query.Skip)
        Assert.AreEqual(5, query.Top)
    End Sub

    <TestMethod> Public Sub WithParameters()
        Dim userParameters1 = New Dictionary(Of String, String)
        userParameters1.Add("state", "PA")
        Dim userParameters2 = New Dictionary(Of String, String)
        userParameters2.Add("country", "USA")

        Dim service = New MobileServiceClient("http://www.test.com")
        Dim table = service.GetTable(Of Product)()

        Dim query = (From p In table Select p).WithParameters(userParameters1).Skip(2).WithParameters(userParameters2)
        Assert.AreEqual(2, query.Parameters.Count)
        Assert.AreEqual("PA", query.Parameters("state"))
        Assert.AreEqual("USA", query.Parameters("country"))
    End Sub

    <TestMethod> Public Sub Filtering()
        ' Greater than, decimal value
        Dim query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Where p.Price > 50 _
                                                     Select p)
        Assert.AreEqual("(Price gt 50M)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Where(Function(p) p.Price > 50))
        Assert.AreEqual("(Price gt 50M)", query.Filter)

        ' Less than, float value
        query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Where p.Weight < 10 _
                                                     Select p)
        Assert.AreEqual("(Weight lt 10f)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Where(Function(p) p.Weight < 10))
        Assert.AreEqual("(Weight lt 10f)", query.Filter)

        ' Less than or equal, equal, And/AndAlso
        query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Where p.Weight <= 10 And p.InStock = True _
                                                     Select p)
        Assert.AreEqual("((Weight le 10f) and (InStock eq true))", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Where(Function(p) p.Weight <= 10 AndAlso p.InStock = True))
        Assert.AreEqual("((Weight le 10f) and (InStock eq true))", query.Filter)

        ' Less than or equal, equal, Or/OrElse
        query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Where p.Weight <= 10 Or p.InStock = True _
                                                     Select p)
        Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Where(Function(p) p.Weight <= 10 OrElse p.InStock))
        Assert.AreEqual("((Weight le 10f) or InStock)", query.Filter)

        'boolean not
        query = Compile(Of Product, Product)(Function(table) _
                                                     From p In table _
                                                     Where Not p.InStock _
                                                     Select p)
        Assert.AreEqual("not(InStock)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Where(Function(p) Not p.InStock))
        Assert.AreEqual("not(InStock)", query.Filter)

        ' Allow New operations
        Dim foo As Single = 10
        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Where(Function(p) p.Weight <= New Product() With {.Weight = foo}.Weight Or p.InStock = True))
        Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                         table.Where(Function(p) p.Weight <= New Product(15) With {.Weight = foo}.Weight Or p.InStock = True))
        Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                         table.Where(Function(p) p.Weight <= New Product(15) With {.Weight = 10}.Weight Or p.InStock = True))
        Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter)

        Dim id As Long = 15
        query = Compile(Of Product, Product)(Function(table) _
                                         table.Where(Function(p) p.Weight <= New Product(id) With {.Weight = 10}.Weight Or p.InStock = True))
        Assert.AreEqual("((Weight le 10f) or (InStock eq true))", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                 table.Where(Function(p) p.Created = New DateTime(1994, 10, 14, 0, 0, 0, DateTimeKind.Utc)))
        Assert.AreEqual("(Created eq datetime'1994-10-14T00:00:00.000Z')", query.Filter)
    End Sub

    <TestMethod> Public Sub CombinedQuery()
        'Dim query = Compile(Of Product, Product)(Function(table) _
        '                                             (From p In table _
        '                                             Where p.Price <= 10 and p.Weight > 10f _
        '                                             where !p.InStock _
        '                                             order by p.Price descending, p.Name _
        '                                             select New With { .Name = p.Name, .Price = p.Price })
        '.Skip(20) _
        '.Take(10))
        Dim query = Compile(Function(table As IMobileServiceTable(Of Product)) _
                                                     table.Where(Function(p) p.Price <= 10 And p.Weight > 10) _
                                                     .Where(Function(p) Not p.InStock) _
                                                     .OrderByDescending(Function(p) p.Price) _
                                                     .ThenBy(Function(p) p.Name) _
                                                     .Select(Function(p) New With {.Name = p.Name, .Price = p.Price}) _
                                                     .Skip(20) _
                                                     .Take(10))
        Assert.AreEqual( _
            "$filter=((Price le 10M) and (Weight gt 10f)) and not(InStock)&$orderby=Price desc,Name&$skip=20&$top=10&$select=Name,Price,Weight,WeightInKG", _
            query.ToQueryString())
    End Sub

    <TestMethod> Public Sub FilterOperators()
        Dim query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name + "x" = "mx" _
                                                    Select p)
        Assert.AreEqual("(concat(Name,'x') eq 'mx')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                            From p In table _
                                                            Where p.Weight + 1.0 = 10 _
                                                            Select p)
        Assert.AreEqual("((Weight add 1) eq 10)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                            From p In table _
                                                            Where p.Weight - 1.0 = 10 _
                                                            Select p)
        Assert.AreEqual("((Weight sub 1) eq 10)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                            From p In table _
                                                            Where p.Weight * 2.0 = 10 _
                                                            Select p)
        Assert.AreEqual("((Weight mul 2) eq 10)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                            From p In table _
                                                            Where p.Weight / 2.0 = 10 _
                                                            Select p)
        Assert.AreEqual("((Weight div 2) eq 10)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                            From p In table _
                                                            Where p.Id Mod 2 = 1 _
                                                            Select p)
        Assert.AreEqual("((id mod 2L) eq 1L)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                            From p In table _
                                                            Where (p.Weight * 2.0) / 3.0 + 1.0 = 10 _
                                                            Select p)
        Assert.AreEqual("((((Weight mul 2) div 3) add 1) eq 10)", query.Filter)
    End Sub

    <TestMethod> Public Sub FilterMethods()
        ' Methods that look like properties
        Dim query = Compile(Of Product, Product)(Function(table) _
                                                            From p In table _
                                                            Where p.Name.Length = 7 _
                                                            Select p)
        Assert.AreEqual("(length(Name) eq 7)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Created.Day = 7
                                                    Select p)
        Assert.AreEqual("(day(Created) eq 7)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Created.Month = 7
                                                    Select p)
        Assert.AreEqual("(month(Created) eq 7)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Created.Year = 7
                                                    Select p)
        Assert.AreEqual("(year(Created) eq 7)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Created.Hour = 7
                                                    Select p)
        Assert.AreEqual("(hour(Created) eq 7)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Created.Minute = 7
                                                    Select p)
        Assert.AreEqual("(minute(Created) eq 7)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Created.Second = 7
                                                    Select p)
        Assert.AreEqual("(second(Created) eq 7)", query.Filter)

        ' Static methods
        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where Math.Floor(p.Weight) = 10 _
                                                    Select p)
        Assert.AreEqual("(floor(Weight) eq 10)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where Decimal.Floor(p.Price) = 10 _
                                                    Select p)
        Assert.AreEqual("(floor(Price) eq 10M)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where Math.Ceiling(p.Weight) = 10 _
                                                    Select p)
        Assert.AreEqual("(ceiling(Weight) eq 10)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where Decimal.Ceiling(p.Price) = 10 _
                                                    Select p)
        Assert.AreEqual("(ceiling(Price) eq 10M)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where Math.Round(p.Weight) = 10 _
                                                    Select p)
        Assert.AreEqual("(round(Weight) eq 10)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where Math.Round(p.Price) = 10 _
                                                    Select p)
        Assert.AreEqual("(round(Price) eq 10M)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where String.Concat(p.Name, "x") = "mx" _
                                                    Select p)
        Assert.AreEqual("(concat(Name,'x') eq 'mx')", query.Filter)

        ' Instance methods
        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.ToLower() = "a" _
                                                    Select p)
        Assert.AreEqual("(tolower(Name) eq 'a')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.ToLowerInvariant() = "a" _
                                                    Select p)
        Assert.AreEqual("(tolower(Name) eq 'a')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.ToUpper() = "A" _
                                                    Select p)
        Assert.AreEqual("(toupper(Name) eq 'A')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.ToUpperInvariant() = "A" _
                                                    Select p)
        Assert.AreEqual("(toupper(Name) eq 'A')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.Trim() = "a" _
                                                    Select p)
        Assert.AreEqual("(trim(Name) eq 'a')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.StartsWith("x") _
                                                    Select p)
        Assert.AreEqual("startswith(Name,'x')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.EndsWith("x") _
                                                    Select p)
        Assert.AreEqual("endswith(Name,'x')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.IndexOf("foo") = 2 _
                                                    Select p)
        Assert.AreEqual("(indexof(Name,'foo') eq 2)", query.Filter)

        Dim c As Char = "x"c
        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.IndexOf(c) = 2 _
                                                    Select p)
        Assert.AreEqual("(indexof(Name,'x') eq 2)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.Contains("bar") _
                                                    Select p)
        Assert.AreEqual("substringof('bar',Name)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.Replace("a"c, "A"c) = "ABBA" _
                                                    Select p)
        Assert.AreEqual("(replace(Name,'a','A') eq 'ABBA')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.Replace("a", "A") = "ABBA" _
                                                    Select p)
        Assert.AreEqual("(replace(Name,'a','A') eq 'ABBA')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.Substring(6) = "world" _
                                                    Select p)
        Assert.AreEqual("(substring(Name,6) eq 'world')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where p.Name.Substring(2, 3) = "llo" _
                                                    Select p)
        Assert.AreEqual("(substring(Name,2,3) eq 'llo')", query.Filter)

        ' Verify each type works on nested expressions too
        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where (p.Name + "x").Length = 7 _
                                                    Select p)
        Assert.AreEqual("(length(concat(Name,'x')) eq 7)", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where String.Concat(p.Name + "x", "y") = "mxy" _
                                                    Select p)
        Assert.AreEqual("(concat(concat(Name,'x'),'y') eq 'mxy')", query.Filter)

        query = Compile(Of Product, Product)(Function(table) _
                                                    From p In table _
                                                    Where (p.Name + "x").ToLower = "ax" _
                                                    Select p)
        Assert.AreEqual("(tolower(concat(Name,'x')) eq 'ax')", query.Filter)
    End Sub
End Class

Public Class Product

    Public Sub New()
    End Sub

    Public Sub New(ByVal id As Long)
        Me.Id = id
    End Sub

    Public Property AvailableTime As TimeSpan
    Public Property Created As DateTime
    Public Property DisplayAisle As Short
    Public Property Id As Long
    Public Property InStock As Boolean
    Public Property Name As String
    Public Property OptionFlags As Byte
    Public Property Price As Decimal
    Public Property SmallId As Integer
    Public Property Tags As List(Of String)
    Public Property Type As ProductType
    Public Property UnsignedDisplayAisle As UInt16
    Public Property UnsignedId As UInt64
    Public Property UnsignedSmallId As UInt32
    Public Property Updated As DateTime?
    <JsonProperty(Required:=Required.Always)> _
    Public Property Weight As Single
    <JsonProperty(Required:=Required.AllowNull)> _
    Public Property WeightInKG As Single?

End Class

Public Enum ProductType
    Food
    Furniture
End Enum