dotnet restore "../src/XProxy.Core.DotNetCore.csproj"
dotnet build -f netcoreapp1.1 "../src/XProxy.Core.DotNetCore.csproj"

dotnet restore "../src/XProxy.DotNetCore.csproj"
dotnet build -f netcoreapp1.1 "../src/XProxy.DotNetCore.csproj"