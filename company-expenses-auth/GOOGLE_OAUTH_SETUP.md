# Google OAuth Setup Guide

This guide explains how to set up Google OAuth authentication for Company Expenses.

## Prerequisites

- A Google Cloud Platform account
- Access to the Google Cloud Console

## Step 1: Create Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to **APIs & Services** > **Credentials**

## Step 2: Configure OAuth Consent Screen

1. Go to **APIs & Services** > **OAuth consent screen**
2. Select **External** (or Internal if using Google Workspace)
3. Fill in the required information:
   - App name: `Company Expenses`
   - User support email: Your email
   - Developer contact email: Your email
4. Add scopes:
   - `email`
   - `profile`
   - `openid`
5. Save and continue

## Step 3: Create OAuth Credentials

1. Go to **APIs & Services** > **Credentials**
2. Click **+ CREATE CREDENTIALS** > **OAuth client ID**
3. Select **Web application**
4. Configure:
   - Name: `Company Expenses Auth Server`
   - Authorized JavaScript origins:
     - `https://localhost:7169` (development)
     - Your production URL
   - Authorized redirect URIs:
     - `https://localhost:7169/signin-google` (development)
     - `https://your-production-url/signin-google` (production)
5. Click **Create**
6. Copy the **Client ID** and **Client Secret**

## Step 4: Configure Application

Add the credentials to your configuration:

### Development (appsettings.Development.json)

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    }
  }
}
```

### Production

Use environment variables or secrets management:

```bash
Authentication__Google__ClientId=YOUR_GOOGLE_CLIENT_ID
Authentication__Google__ClientSecret=YOUR_GOOGLE_CLIENT_SECRET
```

## How It Works

### Registration Flow (Invitation-based)

1. User receives invitation email with a link
2. On the registration page:
   - **Gmail addresses** (@gmail.com, @googlemail.com): Shows "Continue with Google" button
   - **Other emails**: Only shows password registration form
3. When using Google OAuth:
   - The system validates that the Google account email matches the invited email
   - If emails don't match, an error is shown
   - If emails match, the user is automatically registered and signed in

### Login Flow

1. All users see the "Continue with Google" button on the login page
2. Existing users with linked Google accounts can sign in directly
3. New users attempting to sign in via Google are prompted to complete registration

## Security Notes

- The invited email must exactly match the Google account email
- Email is automatically confirmed when using OAuth
- Users can only register via Google if they were invited with a Gmail address
- The invitation token is validated during OAuth registration
