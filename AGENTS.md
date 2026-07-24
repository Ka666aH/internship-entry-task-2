# AGENTS.md — internship-entry-task-2

## Проект

Платёжный сервис (тестовое задание). ASP.NET Core, .NET 10, PostgreSQL 18 (EF Core / Npgsql).

## Роуты (контракт, менять нельзя)

| Метод | Маршрут | Статус |
|---|---|---|
| GET | `/health` | 200 |
| POST | `/operations` | 201 / 409 |
| POST | `/operations/{id}/submit` | 202 / 200 |
| POST | `/receipts` | 204 / 409 |
| GET | `/operations/{id}` | 200 |
| GET | `/operations/{id}/events` | 200 |

## Команды

```bash
dotnet build App/App/App.csproj
docker compose up --build    # полный запуск с PostgreSQL и провайдером
```

## Структура (факт из .csproj и кода)

```
App/App/
  Domain/          — Operation, OperationStatus (enum, SnakeCaseUpper JSON)
  Application/     — пусто, сюда сервисы
  Infrastructure/
    Database/      — AppDbContext (EF Core, operations table)
    Repositories/  — OperationRepository (скелет)
  Presentation/Controllers/ — пусто, сюда контроллеры
  Program.cs       — AddControllers, ConfigureHttpJsonOptions, EnsureCreated
```

## Архитектурные решения (уже зафиксированы в коде)

- **PostgreSQL** через `UseNpgsql`. ConnectionString из `ConnectionStrings:PostgreSQL`.
- `Program.cs:23-28` — `Database.EnsureCreated()` на старте (не миграции).
- Enums сериализуются `SnakeCaseUpper` (`PROCESSING`, `COMPLETED`, `REJECTED`).
- `Operation.SetProviderPaymentId()` кидает `InvalidOperationException` при повторной записи.
- `docker-compose.yml` — 3 сервиса: `candidate-service` (build ./App/App), `db` (postgres:18), `provider-simulator` (ghcr).
- `PROVIDER_URL=http://provider-simulator:8081`.
- Callback: `POST /receipts`, приходит от провайдера на `http://candidate-service:8080/receipts`.
- Submit транзакция: `UPDLOCK` / `UPDATE ... WHERE status='CREATED'`; HTTP вызов снаружи транзакции.
- BackgroundService — дожималка (PROCESSING → retry отправки). Нужно реализовать.

## Что ещё нужно реализовать

- Контроллеры в `Presentation/Controllers/`
- Application-сервисы (создание, submit, обработка receipt, получение events)
- `OperationRepository` — дописать методы
- BackgroundService для фоновой отправки PROCESSING
- Graceful shutdown (проверять `stoppingToken` между итерациями)

## Docker / окружение

- build context: `./App/App` (не корень репо)
- PostgreSQL credentials: `postgres` / `postgres`, БД `operations` (volume `pgdata`)
- `ASPNETCORE_HTTP_PORTS=8080` (из `docker-compose.override.yml`)

## Другое

- nullable включён, implicit usings.
- README, журналы — на русском. Код — на английском.
- `PLAN.md` и `README.old.md` в `.gitignore` — не коммитить.
