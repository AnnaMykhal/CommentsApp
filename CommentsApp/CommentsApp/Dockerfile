# Цей етап використовується при запуску з VS в швидкому режимі (за замовчуванням для конфігурації налагодження)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

## Додайте файл wait-for-it.sh до контейнера
#COPY ./wait-for-it.sh /wait-for-it.sh
#
## Надайте файл прав на виконання
#RUN chmod +x /wait-for-it.sh

# Копіюємо wait-for-it.sh з локального каталогу в контейнер
COPY ./wait-for-it.sh /app/wait-for-it.sh

# Встановлюємо netcat
RUN apt-get update && apt-get install -y netcat-openbsd

# Надаємо права на виконання
RUN chmod +x /app/wait-for-it.sh

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
#ENTRYPOINT ["dotnet", "CommentsApp.dll"]
ENTRYPOINT ["/app/wait-for-it.sh", "rabbitmq_container:5672", "--", "dotnet", "CommentsApp.dll"]
ENV RABBITMQ_URI=amqp://guest:guest@rabbitmq_container:5672

ENV DOTNET_FLUSH_CONSOLE_OUTPUT=true
