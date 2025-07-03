# ---------- build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish -c Release -o /app

# ---------- runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

# ���e����~�uť 8084
ENV ASPNETCORE_URLS=http://0.0.0.0:8084
EXPOSE 8084

ENTRYPOINT ["dotnet", "ResizeWork.dll"]