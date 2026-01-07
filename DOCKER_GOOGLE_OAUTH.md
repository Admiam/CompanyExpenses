# Google OAuth Setup for Docker

When running the application in Docker, you need to configure Google OAuth with the correct redirect URIs.

## Required Redirect URIs in Google Cloud Console

1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
2. Select your OAuth 2.0 Client ID
3. Add the following URIs:

### For Docker (HTTP - Development/Local)

**Authorized JavaScript origins:**
- `http://localhost:5169`

**Authorized redirect URIs:**
- `http://localhost:5169/signin-google`

### For Development (HTTPS - Direct .NET run)

**Authorized JavaScript origins:**
- `https://localhost:7169`

**Authorized redirect URIs:**
- `https://localhost:7169/signin-google`

### For Production

**Authorized JavaScript origins:**
- `https://your-domain.com`

**Authorized redirect URIs:**
- `https://your-domain.com/signin-google`

## Current Configuration

Make sure your `.env` file has the Google OAuth credentials:
```
GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-client-secret
```

## Testing

After adding the redirect URIs to Google Cloud Console:
1. Restart the containers: `docker compose restart auth`
2. Navigate to: http://localhost:5169
3. Try signing in with Google

## Important Notes

- Google OAuth requires HTTPS in production
- For local Docker development, HTTP is acceptable if configured in Google Console
- Wait 5-10 minutes after changing Google Console settings for changes to propagate
