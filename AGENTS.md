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

## Фактическая архитектура

```
App/App/
  Domain/                          — Operation, OperationStatus (enum)
  Application/
    OperationCreateRequest.cs      — record (OperationId, Amount, Currency, Description)
    SubmitResult.cs                — enum Success / Submitted / NotFound
    ReceiptRequest.cs              — record (ProviderPaymentId, OperationId, Result, Message, OccurredAt)
    ReceiptResult.cs               — enum Success / Conflict / Processed / NotFound
    ProviderResponse.cs            — record (ProviderPaymentId, Status)
    Interfaces/Repositories/       — IOperationRepository, IUnitOfWork
    Interfaces/Services/           — IOperationService, IReceiptService
    Services/
      OperationService.cs          — CreateAsync (ловит DbUpdateException → null = 409), SubmitAsync (FOR UPDATE)
      ReceiptService.cs            — ProcessAsync (FOR UPDATE, проверка ProviderPaymentId, first-wins)
      OperationOutboxWorker.cs     — BackgroundService: дожималка PROCESSING → provider
  Infrastructure/
    Database/AppDbContext.cs       — EF Core, DbSet<Operation>
    Repositories/
      OperationRepository.cs       — GetWithLockAsync (SELECT ... FOR UPDATE), CreateAsync, GetAsNoTrackingAsync, GetByStatusAsync
      UnitOfWork.cs                — SaveChangesAsync, BeginTransactionAsync
  Presentation/Controllers/
    OperationController.cs         — Create (409 на дубликат), Submit (202/200), Get (200/404), GetEvents (NotImplemented)
    ReceiptController.cs           — Receive (204/409/404)
  Program.cs                       — DI: репозитории, UnitOfWork, сервисы, HttpClient("Provider"), OperationOutboxWorker, health checks, EnsureCreated

Только одна недоделка: `OperationController.GetEvents` → `throw new NotImplementedException()`.
```

## Зафиксированные решения

- **PostgreSQL** через `UseNpgsql`. ConnectionString из `ConnectionStrings:PostgreSQL`.
- `Program.cs:44-49` — `Database.EnsureCreated()` на старте (не миграции).
- Enums сериализуются `SnakeCaseUpper` (`PROCESSING`, `COMPLETED`, `REJECTED`).
- `Operation.SetProviderPaymentId()` кидает `InvalidOperationException("ProviderPaymentId is already set.")` при повторной записи.
- Submit-транзакция: `SELECT ... FOR UPDATE` (не `UPDLOCK` — это PostgreSQL), HTTP-вызов снаружи транзакции.
- Callback first-wins: если статус не `Processing`, квитанция игнорируется (ReceiptResult.Processed).
- `OperationOutboxWorker` — `BackgroundService`: Parallel.ForEachAsync(max 5), exponential backoff + jitter (5-60s).

## Docker / окружение

- `docker-compose.yml` — 3 сервиса: `candidate-service` (context: `./App/App`), `db` (postgres:18), `provider-simulator` (ghcr).
- `PROVIDER_URL=http://provider-simulator:8081` передаётся через env.
- Callback на `http://candidate-service:8080/receipts`.
- `ASPNETCORE_HTTP_PORTS=8080` (из `docker-compose.override.yml`).
- PostgreSQL: `postgres` / `postgres`, БД `operations`, volume `pgdata`.

## Другое

- `PLAN.md` и `README.old.md` в `.gitignore` — не коммитить.
- Код на английском, README/журналы — на русском.