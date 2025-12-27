# Email Configuration

## Možnosti pro email server:

### 1. Gmail (Doporučeno pro development)

1. **Vytvoř App Password pro Gmail:**

   - Jdi na https://myaccount.google.com/apppasswords
   - Přihlaš se do Google účtu
   - Vyber "Mail" a "Other (Custom name)"
   - Zkopíruj vygenerované heslo (16 znaků)

2. **Uprav appsettings.Development.json:**
   ```json
   "EmailSettings": {
     "SmtpHost": "smtp.gmail.com",
     "SmtpPort": "587",
     "SmtpUsername": "tvuj-email@gmail.com",
     "SmtpPassword": "tvoje-app-password",
     "FromEmail": "tvuj-email@gmail.com",
     "FromName": "Company Expenses"
   }
   ```

### 2. Mailtrap (Fake SMTP pro testování)

1. **Registruj se na https://mailtrap.io (zdarma)**
2. **Získej SMTP credentials z dashboardu**
3. **Uprav appsettings.Development.json:**
   ```json
   "EmailSettings": {
     "SmtpHost": "sandbox.smtp.mailtrap.io",
     "SmtpPort": "2525",
     "SmtpUsername": "tvuj-mailtrap-username",
     "SmtpPassword": "8335b0bff9c72a010f2a5d25403162a8",
     "FromEmail": "noreply@holu.be",
     "FromName": "Company Expenses"
   }
   ```

### 3. Outlook/Hotmail

```json
"EmailSettings": {
  "SmtpHost": "smtp-mail.outlook.com",
  "SmtpPort": "587",
  "SmtpUsername": "tvuj-email@outlook.com",
  "SmtpPassword": "tvoje-heslo",
  "FromEmail": "tvuj-email@outlook.com",
  "FromName": "Company Expenses"
}
```

## Pro vypnutí emailů (development bez SMTP):

V `Program.cs` změň:

```csharp
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, SmtpEmailSender>();
```

na:

```csharp
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
```

## Co obsahují emaily:

1. **Email Confirmation** - S potvrzovacím linkem pro registraci
2. **Password Reset Link** - Link pro reset hesla
3. **Password Reset Code** - 6-místný kód pro reset hesla

Všechny emaily jsou stylované HTML s vaším brand designem.
