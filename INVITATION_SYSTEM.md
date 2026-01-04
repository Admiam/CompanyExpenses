# Invitation System - Dokumentace

## Přehled

Systém pro posílání pozvánek novým uživatelům přes email. Pozvaný uživatel dostane email s odkazem na registraci, kde vyplní své údaje a vytvoří si účet.

## Jak to funguje

### 1. **Vytvoření pozvánky (Admin)**

Admin v aplikaci (na stránce Users) klikne na "Invite User" a vyplní:

- Email uživatele
- Workplace (volitelné)
- Role (volitelné)

**Co se stane:**

1. API vytvoří nový záznam v tabulce `Invitations`
2. Vygeneruje se bezpečný token (32 bytů, Base64)
3. Nastaví se expiraci na 7 dní
4. Odešle se email na zadanou adresu

### 2. **Email pozvánky**

Uživatel dostane email s:

- Uvítací zprávou
- Informací o workplace (pokud je přiřazena)
- Tlačítkem "Complete Registration"
- Odkazem: `http://localhost:5173/register?token=XYZ123...`
- Upozorněním, že pozvánka vyprší za 7 dní

**Příklad emailu:**

```
Welcome to Company Expenses!

You've been invited to join Company Expenses for workplace Engineering Team.

[Complete Registration Button]

Or copy and paste this link: http://localhost:5173/register?token=abc123...

Note: This invitation will expire in 7 days.
```

### 3. **Registrace uživatele**

Když uživatel klikne na odkaz:

1. **Ověření tokenu:**

   - Frontend volá `GET /api/invitations/verify/{token}`
   - API zkontroluje:
     - Existuje pozvánka s tímto tokenem?
     - Je status `Pending`?
     - Nevypršela?
   - Pokud vše OK, vrátí data pozvánky (včetně emailu a workplace)

2. **Registrační formulář:**

   - Email je předvyplněný a disabled (z pozvánky)
   - Uživatel vyplní:
     - Full Name
     - Password
     - Confirm Password
   - Klikne "Create Account"

3. **Vytvoření účtu:**

   - TODO: Volání na auth server pro vytvoření účtu
   - Po úspěšné registraci se volá `POST /api/invitations/{id}/accept` s userId
   - API označí pozvánku jako `Accepted`
   - Pokud byla přiřazena workplace, přidá se uživatel jako member

4. **Redirect:**
   - Uživatel je přesměrován na login stránku auth serveru
   - Přihlásí se svými nově vytvořenými údaji

## Endpointy API

### Vytvoření pozvánky

```
POST /api/invitations
Body: {
  "email": "user@example.com",
  "workplaceId": "guid-optional",
  "invitedRoleId": "string-optional"
}
```

### Ověření tokenu

```
GET /api/invitations/verify/{token}
Response: {
  "id": "guid",
  "email": "user@example.com",
  "workplace": { "id": "guid", "name": "Team Name" },
  "status": "Pending",
  "expiresAt": "2026-01-11T..."
}
```

### Akceptování pozvánky

```
POST /api/invitations/{id}/accept
Body: {
  "userId": "user-id-from-auth"
}
```

### Znovuodeslání pozvánky

```
POST /api/invitations/{id}/resend
```

- Vygeneruje nový token
- Prodlouží expiraci o dalších 7 dní
- Odešle nový email

## Email konfigurace

V `appsettings.Development.json`:

```json
{
  "EmailSettings": {
    "SmtpHost": "sandbox.smtp.mailtrap.io",
    "SmtpPort": "587",
    "SmtpUsername": "your-username",
    "SmtpPassword": "your-password",
    "FromEmail": "noreply@company.com",
    "FromName": "Company Expenses"
  },
  "AppSettings": {
    "FrontendUrl": "http://localhost:5173"
  }
}
```

**Pro production:**

- Použijte reálný SMTP server (Gmail, SendGrid, AWS SES, etc.)
- Změňte `FromEmail` na vaši firemní adresu
- Aktualizujte `FrontendUrl` na produkční URL

## Testování (Development)

Použijeme **Mailtrap** pro testování emailů:

1. Emaily se neposílají skutečným uživatelům
2. Všechny emaily vidíte v Mailtrap inbox
3. Můžete otestovat vzhled a obsah emailu

**Přihlášení do Mailtrap:**

- URL: https://mailtrap.io
- Credentials jsou v `appsettings.Development.json`

## Stavy pozvánky (InvitationStatus)

- `Pending (0)` - Pozvánka čeká na akceptaci
- `Accepted (1)` - Uživatel se zaregistroval
- `Expired (2)` - Pozvánka vypršela (automaticky po 7 dnech)
- `Cancelled (3)` - Admin zrušil pozvánku

## Bezpečnost

1. **Token:**

   - 32 bytů náhodných dat
   - Base64 kódování
   - Unikátní v databázi

2. **Expiraci:**

   - 7 dní od vytvoření
   - Automatická kontrola při ověření

3. **Jednorázové použití:**
   - Po registraci je status změněn na `Accepted`
   - Token nelze použít znovu

## TODO - Integrace s Auth serverem

Aktuálně je v `RegisterPage.tsx` komentář:

```typescript
// TODO: Call auth server registration endpoint
// Example: await authApi.register(registrationData);
```

**Co je potřeba udělat:**

1. Vytvořit endpoint v auth serveru pro registraci s tokenem
2. Přidat validaci tokenu na auth serveru
3. Po úspěšné registraci zavolat `/api/invitations/{id}/accept` z backendu
4. Vytvořit uživatelský účet v Identity databázi
5. Přidat uživatele do WorkplaceMembers (pokud má přiřazenou workplace)

## Příklad flow

```
1. Admin creates invitation
   → Email sent to user@example.com

2. User clicks link in email
   → Opens http://localhost:5173/register?token=abc123

3. Frontend verifies token
   → GET /api/invitations/verify/abc123
   → Returns invitation data

4. User fills registration form
   → Full Name: "John Doe"
   → Password: "******"

5. User submits form
   → POST to auth server /register
   → Auth server validates and creates account
   → Returns userId

6. Backend accepts invitation
   → POST /api/invitations/{id}/accept
   → Status changed to Accepted
   → User added to WorkplaceMembers

7. User redirected to login
   → https://localhost:7169/Account/Login
```

## Možná vylepšení

1. **Email templating:**

   - Použít Razor/Handlebars pro lepší šablony
   - Přidat více stylování a branding

2. **Notifikace:**

   - Poslat email adminu když uživatel přijme pozvánku
   - Poslat připomenutí před vypršením

3. **Statistiky:**

   - Sledovat míru akceptace pozvánek
   - Dashboard s přehledem čekajících pozvánek

4. **Batch invitations:**
   - Možnost pozvat více uživatelů najednou
   - Import z CSV souboru
