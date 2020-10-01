FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as build
WORKDIR /src
COPY ./MC-PBR-Pipeline.sln ./
COPY ./MC-PBR-Pipeline/MC-PBR-Pipeline.csproj ./MC-PBR-Pipeline/
RUN dotnet restore
COPY ./MC-PBR-Pipeline ./MC-PBR-Pipeline/
WORKDIR /src/MC-PBR-Pipeline
RUN dotnet publish -c Release -o /publish

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build /publish ./
ENTRYPOINT ["./MCPBRP"]
