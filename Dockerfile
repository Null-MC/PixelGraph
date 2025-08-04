FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ./PixelGraph.sln ./
COPY ./MinecraftMappings.NET/MinecraftMappings.NET/MinecraftMappings.NET.csproj ./MinecraftMappings.NET/MinecraftMappings.NET/
COPY ./PixelGraph.Common/PixelGraph.Common.csproj ./PixelGraph.Common/
COPY ./PixelGraph.CLI/PixelGraph.CLI.csproj ./PixelGraph.CLI/

RUN dotnet restore ./PixelGraph.Common/PixelGraph.Common.csproj && \
	dotnet restore ./PixelGraph.CLI/PixelGraph.CLI.csproj

COPY ./MinecraftMappings.NET/MinecraftMappings.NET ./MinecraftMappings.NET/MinecraftMappings.NET/
COPY ./PixelGraph.Common ./PixelGraph.Common/
COPY ./PixelGraph.CLI ./PixelGraph.CLI/

WORKDIR /src/PixelGraph.CLI
RUN dotnet publish -c Release -o /publish \
	--runtime linux-musl-x64 --self-contained true

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

# WORKDIR /app
COPY --from=build /publish /app/

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENTRYPOINT ["/app/PixelGraph"]
