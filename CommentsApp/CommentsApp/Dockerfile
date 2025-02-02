# Цей етап використовується при запуску з VS в швидкому режимі (за замовчуванням для конфігурації налагодження)
# 🔹 Базовий контейнер для запуску додатка
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

# Копіюємо wait-for-it.sh для очікування RabbitMQ
COPY ./wait-for-it.sh /app/wait-for-it.sh

# Встановлюємо netcat та задаємо права на виконання
RUN apt-get update && apt-get install -y netcat-openbsd \
    && chmod +x /app/wait-for-it.sh

EXPOSE 8080
EXPOSE 8081
EXPOSE 5017

# 🔹 Контейнер для збирання коду
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Копіюємо лише .csproj та відновлюємо залежності
COPY ["CommentsApp.csproj", "./"]
RUN dotnet restore "./CommentsApp.csproj"

# Копіюємо весь код і збираємо
COPY . . 
RUN dotnet build "./CommentsApp.csproj" -c ${BUILD_CONFIGURATION} -o /app/build

# 🔹 Контейнер для публікації (Release)
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./CommentsApp.csproj" -c ${BUILD_CONFIGURATION} -o /app/publish /p:UseAppHost=false

# 🔹 Фінальний контейнер для виконання додатку
FROM base AS final
WORKDIR /app

# Копіюємо всі опубліковані файли (включаючи wwwroot)
COPY --from=publish /app/publish . 

# Переконуємося, що wwwroot існує
RUN mkdir -p /app/wwwroot

# Запускаємо додаток з очікуванням RabbitMQ
ENTRYPOINT ["/app/wait-for-it.sh", "rabbitmq_container:5672", "--", "dotnet", "CommentsApp.dll"]

# 🔹 Налаштування змінних оточення
ENV RABBITMQ_URI=amqp://guest:guest@rabbitmq_container:5672
ENV DOTNET_FLUSH_CONSOLE_OUTPUT=true
