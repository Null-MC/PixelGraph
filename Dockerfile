FROM mcr.microsoft.com/dotnet/sdk:5.0 as build
WORKDIR /src
COPY ./PixelGraph.sln ./
#COPY ./MinecraftMappings/MinecraftMappings.csproj ./MinecraftMappings/
COPY ./PixelGraph.Common/PixelGraph.Common.csproj ./PixelGraph.Common/
COPY ./PixelGraph.CLI/PixelGraph.CLI.csproj ./PixelGraph.CLI/
RUN dotnet restore ./PixelGraph.Common/PixelGraph.Common.csproj && \
	dotnet restore ./PixelGraph.CLI/PixelGraph.CLI.csproj
#COPY ./MinecraftMappings ./MinecraftMappings/
COPY ./PixelGraph.Common ./PixelGraph.Common/
COPY ./PixelGraph.CLI ./PixelGraph.CLI/
WORKDIR /src/PixelGraph.CLI
RUN dotnet publish -c Release -o /publish \
	--runtime alpine-x64 --self-contained true \
	/p:PublishTrimmed=true

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
WORKDIR /app
COPY --from=build /publish ./
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENTRYPOINT ["./PixelGraph"]
