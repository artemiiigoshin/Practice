# Event API

Простой сервис для управления мероприятиями на ASP.NET Core Web API.

Решение содержит тестовый проект - Tests (Тесты контроллера и валидации) и IntegrationTests для проверки взаимодействий с БД

Реализованные endpoint:
1)Получить список всех событий
2)Получить событие по ID
3)Создать новое событие
4)Обновить существующее событие
5)Удалить событие
6)Создать бронь для события
7)Получить бронь по ID

Статусы бронирования:
1)Pending
2)Confirmed
3)Rejected

Фоновая обработка:
- новые брони создаются со статусом Pending
- через несколько секунд статус меняется на Confirmed

Для получения всех событий реализованы фильтры по имени и датам, а также постраничный вывод.

Интеграция с PostgreSQL через appsettings.json с использованием DefaultConnection
Схема БД управляется миграциями.
Доступ к данным реализуется через репозитории.

Тесты используют БД из контейнера.
Присутствуют интеграционные тесты.
# Запуск проекта

1. Клонируйте репозиторий:
git clone https://github.com/artemiiigoshin/Practice.git
cd <папка проекта>
2. Запустите проект: dotnet run

\#Swagger
Откройте Swagger UI:
https://localhost:<порт из launchSettings.json>
или
http://localhost:<порт из launchSettings.json>
Swagger доступе в Development и Stage

# Архитектура проекта

Проект построен по принципам Clean Architecture и разделён на 4 слоя:

## Practice.Domain
Содержит бизнес-сущности и бизнес-логику:
- Event
- Booking
- EventSeatManager
- EventValidator
- исключения доменной области

Этот слой не зависит от других слоёв.

## Practice.Application
Содержит сценарии использования приложения:
- сервисы (EventService, BookingService)
- DTO
- интерфейсы репозиториев

Зависит только от Practice.Domain.

## Practice.Infrastructure
Содержит реализацию доступа к данным и инфраструктурные компоненты:
- AppDbContext
- репозитории
- миграции EF Core
- background services

Зависит от Practice.Application и Practice.Domain.

## Practice.Presentation
Содержит Web API:
- контроллеры
- middleware
- конфигурацию приложения

Зависит от всех остальных слоёв.

# Создание миграций

Создание миграции:

dotnet ef migrations add MigrationName \
    --project Practice.Infrastructure \
    --startup-project Practice.Presentation

Применение миграций:

dotnet ef database update \
    --project Practice.Infrastructure \
    --startup-project Practice.Presentation

