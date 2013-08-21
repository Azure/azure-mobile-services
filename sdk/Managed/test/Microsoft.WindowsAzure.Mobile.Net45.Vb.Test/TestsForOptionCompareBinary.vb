Option Compare Binary

<TestClass> Public Class TestsForOptionCompareBinary
    <TestMethod> Public Sub StringComparison_OptionCompareBinary()
        Dim query = QueryTests.Compile(Of Product, Product)(Function(table) _
                                                                table.Where(Function(x) x.Name = "Zumo"))
        Assert.AreEqual("(Name eq 'Zumo')", query.Filter)

        query = QueryTests.Compile(Of Product, Product)(Function(table) _
                                                                        table.Where(Function(x) x.Name <> "Zumo"))
        Assert.AreEqual("(Name ne 'Zumo')", query.Filter)
    End Sub
End Class
