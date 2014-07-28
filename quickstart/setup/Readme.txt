"Setup" project is used to make changes in quickstarts source files according to:
- Runtime (DOTNET or NODE)
- Managed SDK version
- WinJS SDK version

Explample:
msbuild Setup.csproj /t:TransformAll /p:Runtime=NODE /p:ManagedSDKVersion=1.2.3 /p:WinJSSDKVersion=1.2.2