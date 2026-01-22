# OneDrive/SharePoint CSV File Picker Demo

A .NET 6.0 ASP.NET Core application demonstrating the recommended replacement strategies for the deprecated `SharedWithMe` Graph API.

## Overview

This demo application shows how to use:
1. **OneDrive File Picker v8** - For interactive file selection from OneDrive and SharePoint
2. **Shared With Me** - Access files shared with the user (replacement for deprecated SharedWithMe API)
3. **Microsoft Graph API** - For file downloads with proper authentication
4. **Client-side MSAL.js** - For token acquisition in the browser
5. **API Request Logging** - Server-side logging of all API calls for debugging

## Prerequisites

- .NET 6.0 SDK or later
- Azure AD app registration (already registered with ID: `YOUR-CLIENT-ID`)

## App Registration Configuration

Configure your Azure AD app registration with the following settings:

### Redirect URIs

Add the following as **Web** redirect URIs:
- `https://localhost:7235/signin-oidc`

### Platform Configuration

Under **Authentication**:
1. Select **Web** platform
2. Add redirect URI: `https://localhost:7235/signin-oidc`
3. Set Front-channel logout URL: `https://localhost:7235/signout-oidc`
4. Check **ID tokens** (for implicit grant and hybrid flows)
5. Check **Access tokens** (for implicit grant and hybrid flows)

### API Permissions

Add the following **Delegated** permissions:

| API | Permission | Purpose |
|-----|------------|---------|
| **Microsoft Graph** | `User.Read` | Read user profile |
| **Microsoft Graph** | `Files.Read.All` | Read all files user has access to |
| **Microsoft Graph** | `Sites.Read.All` | Read SharePoint sites |
| **SharePoint** | `MyFiles.Read` | Access OneDrive files via File Picker |
| **SharePoint** | `AllSites.Read` | Access SharePoint sites via File Picker |

**Important:** After adding permissions, click "Grant admin consent" if you have admin privileges, or request consent from your tenant administrator.

### How to Add SharePoint Permissions

1. Go to **API permissions** → **Add a permission**
2. Select **APIs my organization uses**
3. Search for **Office 365 SharePoint Online** (or just "SharePoint")
4. Select **Delegated permissions**
5. Add:
   - `MyFiles.Read`
   - `AllSites.Read`

## Running the Application

1. Clone this repository
2. Navigate to the project directory
3. Copy `appsettings.json` to `appsettings.Development.json` and update with your values:
   - Azure AD Client ID and Tenant ID
   - SharePoint URLs for your tenant
4. Restore packages and run:

```bash
dotnet restore
dotnet run
```

5. Open your browser to `https://localhost:7235`
6. Sign in with your Microsoft account
7. Click "Launch File Picker" to start browsing files

## Configuration

### appsettings.Development.json

Create this file (gitignored) with your environment-specific settings:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourdomain.onmicrosoft.com",
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "CallbackPath": "/signin-oidc"
  },
  "SharePoint": {
    "OdspBaseUrl": "https://yourtenant.sharepoint.com",
    "MyFilesBaseUrl": "https://yourtenant-my.sharepoint.com"
  }
}
```

## How It Works

### File Picker Integration

The OneDrive File Picker v8 is a Microsoft-hosted control that:
- Displays files from the user's OneDrive and SharePoint
- Shows files shared with the user (replacement for `SharedWithMe` API)
- Communicates with your app via `postMessage` API
- Returns file metadata that you can use to download the file content

### Authentication Flow

1. User signs in via Microsoft Identity (OpenID Connect, ID token only)
2. MSAL.js initializes in the browser with the app configuration
3. When File Picker opens, MSAL.js acquires tokens via popup for SharePoint
4. App provides tokens to the picker via the postMessage API
5. User selects a file → app receives file metadata
6. MSAL.js acquires Graph token, app downloads file via Microsoft Graph API
7. All API requests are logged to server-side CSV files for debugging

### API Logging

The application logs all client-side API requests to server-side CSV files in the `Logs/` folder:
- Logs include: request ID, timestamp, method, URL, status, duration
- Bearer tokens are automatically redacted for security
- Logs are useful for debugging file picker and Graph API interactions

### File Picker Entry Points

The demo supports three entry points:

| Entry Point | Description |
|-------------|-------------|
| **OneDrive Files** | User's personal OneDrive |
| **SharePoint Sites** | SharePoint document libraries |
| **Shared With Me** | Files shared with the user (SharedWithMe replacement!) |

## Scanning Guidance (For Background Processing)

For scenarios requiring background file scanning without user interaction, Microsoft recommends:

### Delta Query Pattern

```csharp
// Get initial list of all files
GET /sites/{siteId}/drive/root/delta

// Later, get only changes since last sync
GET /sites/{siteId}/drive/root/delta?token={deltaToken}
```

### Webhook Subscriptions

```csharp
// Subscribe to drive changes
POST /subscriptions
{
    "changeType": "updated",
    "notificationUrl": "https://yourapp.com/webhook",
    "resource": "/drives/{driveId}/root",
    "expirationDateTime": "2024-12-31T00:00:00Z"
}
```

### Site Enumeration

```csharp
// List all sites (requires Application permissions for full enumeration)
GET /sites?search=*

// Get drives in a site
GET /sites/{siteId}/drives
```

## Project Structure

```
ScannerAndPicker/
├── Controllers/
│   ├── HomeController.cs        # Main controller with token endpoint
│   ├── CsvViewerController.cs   # CSV file download and parsing
│   └── ApiLogController.cs      # API logging endpoint
├── Models/
│   ├── FilePickerViewModel.cs   # View model for file picker
│   ├── CsvDataViewModel.cs      # View model for CSV display
│   └── ErrorViewModel.cs        # Error handling model
├── Services/
│   └── ApiLogService.cs         # Server-side API request/response logging
├── Views/
│   ├── Home/
│   │   ├── Index.cshtml         # Landing page
│   │   ├── FilePicker.cshtml    # File picker with MSAL.js integration
│   │   └── AuthRedirect.cshtml  # MSAL popup redirect handler
│   ├── CsvViewer/
│   │   └── DisplayFromSession.cshtml  # CSV table display
│   └── Shared/
│       ├── _Layout.cshtml       # Main layout
│       └── Error.cshtml         # Error page
├── Logs/                        # API log files (gitignored)
├── Program.cs                   # App configuration
├── appsettings.json            # Configuration template
├── appsettings.Development.json # Development settings (gitignored)
├── .gitignore                  # Git ignore rules
└── README.md                   # This file
```

## Troubleshooting

### "Popup blocked" error
Allow popups for `localhost` in your browser settings.

### Token acquisition fails
Ensure:
1. All required API permissions are granted
2. Admin consent is provided for the permissions
3. The redirect URI matches exactly (`https://localhost:7235/signin-oidc`)

### File Picker doesn't load
Check:
1. The base URL is correct for your OneDrive/SharePoint tenant
2. The access token is valid for the SharePoint resource
3. Browser console for detailed error messages

### CORS errors
The File Picker runs in a popup window and communicates via `postMessage`. If you see CORS errors, ensure you're using the popup approach rather than iframe embedding.

## References

- [OneDrive File Picker v8 Documentation](https://learn.microsoft.com/en-us/onedrive/developer/controls/file-pickers/?view=odsp-graph-online)
- [Scanning Guidance](https://learn.microsoft.com/en-us/onedrive/developer/rest-api/concepts/scan-guidance?view=odsp-graph-online)
- [File Picker Configuration Schema](https://learn.microsoft.com/en-us/onedrive/developer/controls/file-pickers/v8-schema?view=odsp-graph-online)
- [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web)
- [Sample Code Repository](https://aka.ms/OneDrive/samples/file-picking)

## License

This demo is provided as-is for educational purposes.
# OD-SP-FilePickerDemo
