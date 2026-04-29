# GitHubExplorer

Веб-приложение для поиска, фильтрации и изучения GitHub репозиториев.  
Построено на **ASP.NET Core 10 MVC** + **Octokit.NET** + **PostgreSQL**.

---

## Стек

| Слой | Технология |
|------|-----------|
| Framework | ASP.NET Core 10 MVC |
| GitHub API | Octokit.NET 13 |
| База данных | PostgreSQL + EF Core 9 |
| Аутентификация | GitHub OAuth 2.0 |
| Кэш | IMemoryCache |
| Логирование | Serilog |

---

## Страницы

| URL | Описание |
|-----|----------|
| `/` | Главная — hero секция, поиск |
| `/explore` | Каталог репозиториев с фильтрами |
| `/trending` | Трендовые репо за день / неделю / месяц |
| `/repo/{owner}/{name}` | Детальная страница репозитория |
| `/user/{login}` | Профиль разработчика |
| `/compare` | Сравнение двух репозиториев |
| `/account` | Избранное и коллекции (требует авторизации) |

---

## Быстрый старт

### 1. Клонируй репозиторий

```bash
git clone https://github.com/YOUR_USERNAME/GitHubExplorer.git
cd GitHubExplorer
```

### 2. Настрой секреты

Открой `appsettings.Development.json` и заполни:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=gitexplorer_dev;Username=postgres;Password=YOUR_PASSWORD"
  },
  "GitHub": {
    "ClientId": "YOUR_GITHUB_CLIENT_ID",
    "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET",
    "ServerToken": "YOUR_GITHUB_PAT_TOKEN"
  }
}
```

**Где взять ключи:**
- `ClientId` / `ClientSecret` — [github.com/settings/developers](https://github.com/settings/developers) → OAuth Apps → New OAuth App  
  - Homepage URL: `http://localhost:5209`  
  - Callback URL: `http://localhost:5209/account/callback`
- `ServerToken` — [github.com/settings/tokens](https://github.com/settings/tokens) → Fine-grained token (public repos — read only)

### 3. Создай базу данных

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 4. Запусти

```bash
dotnet run
```

Открой [http://localhost:5209](http://localhost:5209)

---

## Структура проекта

```
GitHubExplorer/
├── Controllers/         # MVC контроллеры
├── Services/
│   ├── GitHub/          # IGitHubService + GitHubService (Octokit)
│   └── Cache/           # ICacheService + CacheService
├── Models/
│   ├── Domain/          # EF Core сущности (AppUser, FavoriteRepo, Collection)
│   └── ViewModels/      # Данные для View
├── Data/                # AppDbContext + Migrations
├── Views/               # Razor Views
└── wwwroot/             # Статика (CSS, JS)
```

---

## Стратегия кэширования

| Данные | TTL |
|--------|-----|
| Trending репо | 30 мин |
| Детали репозитория | 15 мин |
| Профиль пользователя | 20 мин |
| Результаты поиска | 5 мин |
| README | 1 час |

---

## GitHub API Rate Limit

Без токена GitHub даёт **60 запросов/час**.  
С `ServerToken` (PAT) — **5 000 запросов/час**.  
Кэш значительно снижает количество реальных запросов к API.
