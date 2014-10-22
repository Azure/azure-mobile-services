Option Compare Binary
Imports Microsoft.WindowsAzure.MobileServices.Query

<TestClass> Public Class TestsForOptionCompareBinary
    <TestMethod> Public Sub StringComparison_OptionCompareBinary()
        Dim query = QueryTests.Compile(Of Product, Product)(Function(table) _
                                                                table.Where(Function(x) x.Name = "Zumo"))
        Assert.AreEqual("(Name eq 'Zumo')", ODataExpressionVisitor.ToODataString(query.Filter))

        query = QueryTests.Compile(Of Product, Product)(Function(table) _
                                                                        table.Where(Function(x) x.Name <> "Zumo"))
        Assert.AreEqual("(Name ne 'Zumo')", ODataExpressionVisitor.ToODataString(query.Filter))
    End Sub
End Class
