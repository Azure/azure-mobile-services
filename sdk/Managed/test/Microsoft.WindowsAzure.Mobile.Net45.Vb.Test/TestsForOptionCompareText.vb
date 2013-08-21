Option Compare Text

<TestClass> Public Class TestsForOptionCompareText
    <TestMethod> Public Sub StringComparison_OptionCompareText()
        Dim query = QueryTests.Compile(Of Product, Product)(Function(table) _
                                                                table.Where(Function(x) x.Name = "Zumo"))
        Assert.AreEqual("(tolower(Name) eq tolower('Zumo'))", query.Filter)

        query = QueryTests.Compile(Of Product, Product)(Function(table) _
                                                                        table.Where(Function(x) x.Name <> "Zumo"))
        Assert.AreEqual("(tolower(Name) ne tolower('Zumo'))", query.Filter)
    End Sub
End Class
