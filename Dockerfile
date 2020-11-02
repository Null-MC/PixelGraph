FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY ./PixelGraph.sln ./
COPY ./PixelGraph.Common/PixelGraph.Common.csproj ./PixelGraph.Common/
COPY ./PixelGraph.CLI/PixelGraph.CLI.csproj ./PixelGraph.CLI/
COPY ./PixelGraph.UI/PixelGraph.UI.csproj ./PixelGraph.UI/
COPY ./PixelGraph.Tests/PixelGraph.Tests.csproj ./PixelGraph.Tests/
RUN dotnet restore
COPY ./PixelGraph ./PixelGraph/
WORKDIR /src/PixelGraph.CLI
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /publish ./
ENTRYPOINT ["./PixelGraph"]
