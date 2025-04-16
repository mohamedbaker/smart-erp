# Use official .NET SDK image to build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /Smart_ERP

COPY . . 
RUN dotnet restore

RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /Smart_ERP

COPY --from=build /Smart_ERP/out ./

EXPOSE 5000

ENTRYPOINT ["dotnet", "Smart_ERP.dll"]
