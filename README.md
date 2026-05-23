# Matted Labs Uptime

Internal home lab uptime monitor. Pings configured services on a schedule, logs results to SQL Server, and sends email alerts on state changes.

- **Web UI:** http://localhost:4100
- **API:** http://localhost:5100/api
- **Health check:** http://localhost:5100/api/health

---

## Setup

### 1. Clone and configure

```bash
git clone https://github.com/edwardsbs/matted-labs-uptime.git
cd matted-labs-uptime
cp .env.example .env
```

Edit `.env` — set a strong `SA_PASSWORD` and update the `CONNECTION_STRING` to match.

### 2. Configure SMTP (optional)

Fill in the `SMTP_*` variables in `.env`. Uses standard SMTP with TLS (port 587 by default). Gmail requires an App Password if you have 2FA enabled.

If left blank, email alerts are silently skipped and everything else works normally.

### 3. Start

```bash
docker compose up -d
```

On first start the API applies EF Core migrations and seeds the six default services automatically.

---

## Deploy (on the Ubuntu VM)

```bash
chmod +x deploy.sh
./deploy.sh
```

---

## Adding services to monitor

Open the web UI at http://localhost:4100 → **Settings** → **Add Service**.

Or POST to the API directly:

```bash
curl -X POST http://localhost:5100/api/services \
  -H 'Content-Type: application/json' \
  -d '{"name":"My App","url":"http://myapp:3000/health","isActive":true,"ignoreSslErrors":false,"intervalMinutes":5}'
```

---

## Running migrations manually

If you need to apply migrations without restarting Docker:

```bash
cd matted-labs-uptime-api
dotnet ef database update --connection "Server=localhost,1433;Database=MattedLabsUptime;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
```

---

## Default monitored services

| Name | URL |
|---|---|
| Bartender API | http://bartender-api:8080/health |
| Bartender Web | http://bartender-web:4200 |
| Out The Door | http://out-the-door:3000 |
| Proxmox Dashboard | https://192.168.0.7:8006 (SSL ignored) |
| Ledger API | http://ledger-api:5000/health |
| Ledger Web | http://ledger-web:4200 |

These are seeded on first run. Update URLs in Settings once you verify the correct ports.

---

## Architecture

- **API:** ASP.NET Core 9, EF Core 9, SQL Server
- **Background service:** checks all active services every minute (respects per-service `intervalMinutes`)
- **Alert logic:** one email per state transition (down → up or up → down)
- **Data retention:** checks older than 90 days are pruned automatically
- **Frontend:** Angular 20+, Angular Material dark theme, 30-second auto-refresh
