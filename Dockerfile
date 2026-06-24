# 1. Usa a ferramenta oficial da Microsoft (Versão 8!)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /App

# Copia os teus ficheiros
COPY . ./

# Restaura e prepara a versão final (Release)
RUN dotnet restore
RUN dotnet publish -c Release -o out

# 2. Usa a versão super leve só para correr o bot (Versão 8!)
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /App
COPY --from=build-env /App/out .

# O comando que liga a máquina
ENTRYPOINT ["dotnet", "FinanceBot.dll"]