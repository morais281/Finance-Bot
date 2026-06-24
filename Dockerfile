# Etapa 1: Build (A usar o SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copiar tudo e restaurar
COPY . ./
RUN dotnet restore

# Compilar a versão final
RUN dotnet publish -c Release -o out

# Etapa 2: Runtime (O SEGREDO ESTÁ AQUI: "aspnet" em vez de só "runtime")
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /App
COPY --from=build-env /App/out .

# Arrancar o bot
ENTRYPOINT ["dotnet", "FinanceBot.dll"]