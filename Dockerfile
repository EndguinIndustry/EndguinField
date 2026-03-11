FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EndguinField.csproj ./
RUN dotnet restore ./EndguinField.csproj

COPY . ./
RUN dotnet publish ./EndguinField.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV TZ=Asia/Bangkok
EXPOSE 8080

COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "EndguinField.dll"]
