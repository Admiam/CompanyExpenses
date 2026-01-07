# Company Expenses

Webová aplikace pro správu firemních výdajů. Umožňuje zaměstnancům podávat výdaje, manažerům je schvalovat a administrátorům spravovat celý systém.

## 🚀 Rychlý start (Docker)

### Požadavky

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows, macOS, Linux)

### Spuštění jedním příkazem

**macOS/Linux:**

```bash
./start.sh
```

**Windows (PowerShell):**

```powershell
.\start.ps1
```

**Windows (CMD):**

```cmd
start.bat
```

**Nebo přímo Docker Compose:**

```bash
docker compose up --build -d
```

### Přístup k aplikaci

| Služba                   | URL                   |
| ------------------------ | --------------------- |
| 🌐 **Frontend aplikace** | http://localhost:3000 |
| 🔐 Auth Server           | http://localhost:5169 |
| 📡 API Server            | http://localhost:5200 |

---

## 🔑 Přihlašovací údaje

### Admin účet (vytvořen automaticky)

| Pole      | Hodnota                        |
| --------- | ------------------------------ |
| **Email** | `admin@company-expenses.local` |
| **Heslo** | `Admin123!`                    |

---

## 📋 Testovací scénáře

### 1. Přihlášení jako Admin

1. Otevřete http://localhost:3000
2. Klikněte na **"Přihlásit se"** (budete přesměrováni na Auth server)
3. Zadejte:
   - Email: `admin@company-expenses.local`
   - Heslo: `Admin123!`
4. Po přihlášení máte plný přístup k administraci

### 2. Vytvoření pracoviště (Workplace)

1. V menu klikněte na **"Pracoviště"** nebo **"Workplaces"**
2. Klikněte na **"Nové pracoviště"**
3. Zadejte:
   - Název: `IT oddělení`
   - Kód: `IT-001`
4. Uložte

### 3. Pozvání nového uživatele

1. Jděte do **"Pozvánky"** nebo **"Invitations"**
2. Klikněte na **"Nová pozvánka"**
3. Zadejte email nového uživatele (např. `jan.novak@example.com`)
4. Vyberte roli (User, Manager, Admin)
5. Volitelně přiřaďte k pracovišti
6. Odešlete pozvánku

> **Poznámka:** V Docker prostředí nejsou emaily odesílány. Pro testování můžete registrovat uživatele přímo.

### 4. Registrace nového uživatele

1. Na přihlašovací stránce klikněte na **"Registrovat"**
2. Vyplňte:
   - Email: `test@example.com`
   - Heslo: `Test123!` (min. 6 znaků, velké písmeno, číslo, speciální znak)
3. Potvrďte registraci
4. Přihlaste se novými údaji

### 5. Vytvoření výdaje (Expense)

1. Přihlaste se jako uživatel s přiřazeným pracovištěm
2. Jděte do **"Výdaje"** nebo **"Expenses"**
3. Klikněte na **"Nový výdaj"**
4. Vyplňte:
   - Kategorie: vyberte z nabídky (Travel, Meals, Office Supplies...)
   - Částka: např. `1500`
   - Měna: `CZK`
   - Datum: dnešní datum
   - Popis: `Služební cesta do Brna`
5. Volitelně nahrajte účtenku (obrázek)
6. Uložte

### 6. Schválení výdaje (jako Manager)

1. Přihlaste se jako Manager pracoviště
2. Jděte do **"Výdaje"**
3. Najděte výdaj se statusem **"Pending"**
4. Klikněte na výdaj pro detail
5. Schvalte nebo zamítněte s komentářem

### 7. Nastavení limitů pracoviště

1. Jako Admin jděte do **"Pracoviště"** → vyberte pracoviště
2. Přejděte na záložku **"Limity"**
3. Přidejte nový limit:
   - Období: od-do datum
   - Limit: např. `50000 CZK`
   - Volitelně pro konkrétní kategorii
4. Uložte

### 8. Správa kategorií výdajů

1. Jako Admin jděte do **"Kategorie"** nebo **"Categories"**
2. Můžete:
   - Přidávat nové kategorie
   - Měnit barvy kategorií
   - Deaktivovat kategorie

---

## 🛠️ Užitečné příkazy

### Zobrazení logů

```bash
# Všechny služby
docker compose logs -f

# Konkrétní služba
docker compose logs -f api
docker compose logs -f auth
docker compose logs -f app
```

### Zastavení aplikace

```bash
docker compose down
```

### Kompletní reset (smazání dat)

```bash
docker compose down -v
docker compose up --build -d
```

### Přístup k databázi

```bash
docker compose exec db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "CompanyExpenses123!" -C
```

---

## 📁 Struktura projektu

```
CompanyExpenses/
├── company-expenses-app/     # Frontend (React + Vite + Tailwind)
├── company-expenses-api/     # API Server (ASP.NET Core)
├── company-expenses-auth/    # Auth Server (Blazor + Identity)
├── company-expenses-database/# Database layer (EF Core)
├── company-expenses-models/  # Shared entity models
├── company-expenses-services/# Business logic services
├── docker-compose.yml        # Docker orchestrace
├── start.sh                  # Startup script (macOS/Linux)
├── start.bat                 # Startup script (Windows CMD)
└── start.ps1                 # Startup script (Windows PowerShell)
```

---

## 🔧 Vývoj (bez Dockeru)

### Požadavky

- .NET 10 SDK
- Node.js 20+
- SQL Server (nebo Docker kontejner)

### Spuštění pro vývoj

1. **Databáze** - spusťte SQL Server
2. **Nastavte connection string** v `appsettings.Development.json`
3. **Spusťte služby:**

```bash
# Terminal 1 - Auth server
cd company-expenses-auth
dotnet watch run

# Terminal 2 - API server
cd company-expenses-api
dotnet watch run

# Terminal 3 - Frontend
cd company-expenses-app
npm install
npm run dev
```

Nebo použijte VS Code Tasks: `Ctrl+Shift+P` → `Tasks: Run Task` → `watch-full-stack`

---

## 📧 Konfigurace emailu (volitelné)

Pro odesílání emailů (pozvánky, reset hesla) nastavte SMTP v `appsettings.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "smtp.example.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-username",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@example.com",
    "FromName": "Company Expenses"
  }
}
```

---

## 🔐 Google OAuth (volitelné)

Pro přihlašování přes Google:

1. Vytvořte projekt v [Google Cloud Console](https://console.cloud.google.com/)
2. Nastavte OAuth credentials
3. Přidejte do environment variables:

```bash
export GOOGLE_CLIENT_ID="your-client-id"
export GOOGLE_CLIENT_SECRET="your-client-secret"
docker compose up --build -d
```

---

## 📝 Role uživatelů

| Role        | Oprávnění                                         |
| ----------- | ------------------------------------------------- |
| **User**    | Vytvářet vlastní výdaje, prohlížet stav schválení |
| **Manager** | + Schvalovat/zamítat výdaje členů pracoviště      |
| **Admin**   | + Správa uživatelů, pracovišť, kategorií, limitů  |

---

## ❓ Řešení problémů

### Aplikace se nespustí

```bash
docker compose logs api
docker compose logs auth
```

### Databáze není připravena

```bash
docker compose restart db
# Počkejte 30 sekund
docker compose restart api auth
```

### Čistý start

```bash
docker compose down -v --rmi all
docker compose up --build -d
```

---

## 📄 Licence

Školní projekt - PIAE
