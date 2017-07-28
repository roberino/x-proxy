dotnet restore "src/XProxy.Core/XProxy.Core.DotNetCore.csproj"
dotnet build -f netcoreapp1.1 "src/XProxy.Core/XProxy.Core.DotNetCore.csproj"

dotnet restore "src/XProxy/XProxy.DotNetCore.csproj"
dotnet build -f netcoreapp1.1 "src/XProxy/XProxy.DotNetCore.csproj"