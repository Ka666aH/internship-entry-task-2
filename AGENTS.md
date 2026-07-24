# AGENTS.md — internship-entry-task-2

## Проект

Платёжный сервис (тестовое задание для стажировки). ASP.NET Core, .NET 10.  
HTTP API с идемпотентностью, crash recovery, фоновой дожималкой и callback-квитанциями.

## Роуты (читать README.md — это контракт)

| Метод | Маршрут | Статус |
|---|---|---|
| GET | `/health` | 200 |
| POST | `/operations` | 201 / 409 |
| POST | `/operations/{id}/submit` | 202 / 200 |
| POST | `/receipts` | 204 / 409 |
| GET | `/operations/{id}` | 200 |
| GET | `/operations/{id}/events` | 200 |

Все остальные — на усмотрение. Маршруты выше менять нельзя — по ним идёт автопроверка.

## Архитектура

- `Program.cs` — `builder.Services.AddControllers()` (НЕ Minimal API). Папка `Controllers/` уже создана, нужно добавить контроллеры.
- Обязательное хранилище — любое. SQLite проще всего (volume `/data`). EF Core или ADO.NET — без разницы.
- Фоновая дожималка — `BackgroundService`, читает `PROCESSING` и повторяет отправку.
- Provider — внешний HTTP (из `PROVIDER_URL`). Idempotency-Key = operationId.
- Callback — source of truth. Только он переводит в финальный статус.
- Транзакция submit: `UPDLOCK` / `UPDATE ... WHERE status='CREATED'`, HTTP-вызов снаружи транзакции.

## Команды

```bash
# Сборка
dotnet build App/App/App.csproj

# Запуск с провайдером (из корня репо)
docker compose up --build
```

Сервис слушает порт 8080. Provider-simulator на 8081.

## Docker

- Dockerfile: `App/App/Dockerfile` (multi-stage, net10.0).
- `docker-compose.yml` (или `compose.yaml`) — в КОРНЕ репозитория.
- Образ провайдера: `ghcr.io/fintech-dev-lab/internship-provider-simulator:v0.2.0` (публичный).
- Volume: `candidate-data:/data`.
- `PROVIDER_URL=http://provider-simulator:8081`
- `CALLBACK_URL=http://candidate-service:8080/receipts`

## Окружение

- `PROVIDER_URL` — обязателен, адрес провайдера.
- Порт 8080 в контейнере (в launchSettings для Docker профиля `ASPNETCORE_HTTP_PORTS=8080`).
- Решение: `App/App.slnx` (новый формат .slnx).

## Что ещё не сделано

- Нет ни одного контроллера, модели, миграции.
- Нет compose.yaml в корне (нужен обязательно).
- Нет тестов (dotnet test).
- Выбор БД не зафиксирован.
- Файлы `PLAN.md` и `README.old.md` в .gitignore — не коммитить.

## Стиль

- C#, nullable включён, implicit usings.
- Сообщения, журналы, README — на русском (ТЗ на русском).
- Код — на английском (переменные, классы, комментарии).
