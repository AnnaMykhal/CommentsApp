# Цей етап використовується при запуску з VS в швидкому режимі (за замовчуванням для конфігурації налагодження)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5017

# Цей етап використовується для побудови проекту служби
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# Копіюємо лише .csproj файл у корінь робочої директорії
COPY ["CommentsApp.csproj", "./"]
# Відновлюємо залежності для проекту
RUN dotnet restore "./CommentsApp.csproj"
# Копіюємо всі інші файли проекту в контейнер
COPY . . 
WORKDIR "/src"
# Будуємо проект
RUN dotnet build "./CommentsApp.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

# Цей етап використовується для публікації проекту служби
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CommentsApp.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

# Цей етап використовується в робочому середовищі або при запуску з VS у звичайному режимі
FROM base AS final
WORKDIR /app
# Копіюємо публікацію з попереднього етапу
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "CommentsApp.dll"]



## См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.
#
## В зависимости от операционной системы хост-компьютеров, которые будут выполнять сборку контейнеров или запускать их, может потребоваться изменить образ, указанный в инструкции FROM.
## Дополнительные сведения см. на странице https://aka.ms/containercompat
#
## Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
##FROM mcr.microsoft.com/dotnet/aspnet:8.0-nanoserver-1809 AS base
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
#WORKDIR /app
#EXPOSE 8080
#EXPOSE 8081
#
#
## Этот этап используется для сборки проекта службы
##FROM mcr.microsoft.com/dotnet/sdk:8.0-nanoserver-1809 AS build
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#COPY ["CommentsApp/CommentsApp.csproj", "CommentsApp/"]
#RUN dotnet restore "./CommentsApp/CommentsApp.csproj"
#COPY . .
#WORKDIR "/src/CommentsApp"
#RUN dotnet build "./CommentsApp.csproj" -c %BUILD_CONFIGURATION% -o /app/build
#
## Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
#FROM build AS publish
#ARG BUILD_CONFIGURATION=Release
#RUN dotnet publish "./CommentsApp.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false
#
## Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "CommentsApp.dll"]