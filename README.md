# Study Test Simulator

A comprehensive .NET 10 Blazor Server practice test application with Microsoft 365 EntraId SSO authentication. This application allows users to create, manage, and take practice tests with detailed tracking and analytics.

## Features

- **Microsoft 365 EntraId SSO Authentication** - Single sign-on for your organization
- **Test Management** - Create and manage test categories and questions
- **Multiple Choice Tests** - Support for questions with 4 answers (single correct answer)
- **Image Support** - Questions can include images (stored as base64 in database)
- **Test Taking** - Interactive test interface with timers and immediate feedback
- **JSON Import** - Bulk import questions from JSON files
- **Test History** - Track all test attempts with detailed statistics
- **Question Flagging** - Flag questions for review during tests
- **Review Mode** - Review past test attempts with correct/incorrect answers
- **Time Tracking** - Track session time and per-question time
- **Randomized Questions** - Questions appear in random order for each attempt

## Technology Stack

- **Framework**: .NET 10
- **UI**: Blazor Server with Bootstrap 5
- **Authentication**: Microsoft Identity Web (EntraId SSO)
- **Database**: SQL Server with Entity Framework Core 10
- **Hosting**: Kestrel on Windows Server

## Prerequisites

- .NET 10 SDK
- SQL Server (2019 or later)
- Microsoft 365 Azure AD tenant with admin access
- Windows Server (for production deployment)

## Azure AD App Registration

Before running the application, you need to register it in Azure Active Directory:

### 1. Create App Registration

1. Navigate to [Azure Portal](https://portal.azure.com)
2. Go to **Azure Active Directory** > **App registrations**
3. Click **New registration**
   - **Name**: StudyTestSimulator
   - **Supported account types**: Accounts in this organizational directory only (Single tenant)
   - **Redirect URI**: Select "Web" and enter `https://localhost:5001/signin-oidc` (for development)
4. Click **Register**

### 2. Note Application IDs

After registration, note the following from the **Overview** page:
- **Application (client) ID** - This is your `ClientId`
- **Directory (tenant) ID** - This is your `TenantId`
- **Azure AD domain** - Something like `yourcompany.onmicrosoft.com`

### 3. Create Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
   - **Description**: StudyTestSimulator Secret
   - **Expires**: Choose your preferred expiration
3. Click **Add**
4. **Important**: Copy the secret **Value** immediately - this is your `ClientSecret`

### 4. Configure API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph**
4. Select **Delegated permissions**
5. Add the following permissions:
   - `User.Read`
6. Click **Add permissions**
7. Click **Grant admin consent** (requires admin privileges)

### 5. Add Redirect URIs

For production deployment:
1. Go to **Authentication**
2. Under **Platform configurations** > **Web**, add your production URL:
   - `https://yourdomain.com/signin-oidc`
3. Under **Logout URL**, add:
   - `https://yourdomain.com/signout-callback-oidc`

## Configuration

### 1. Update appsettings.json

Edit `StudyTestSimulator.Web/appsettings.json` and replace the placeholders:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourcompany.onmicrosoft.com",
    "TenantId": "YOUR_TENANT_ID_HERE",
    "ClientId": "YOUR_CLIENT_ID_HERE",
    "ClientSecret": "YOUR_CLIENT_SECRET_HERE",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=StudyTestSimulator;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Important**: For production, use User Secrets or Azure Key Vault to store the `ClientSecret`. Never commit secrets to source control!

### 2. Update Connection String

Modify the `DefaultConnection` string based on your SQL Server setup:

**Windows Authentication**:
```
Server=localhost;Database=StudyTestSimulator;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

**SQL Authentication**:
```
Server=localhost;Database=StudyTestSimulator;User Id=your_user;Password=your_password;TrustServerCertificate=True;MultipleActiveResultSets=true
```

**Azure SQL**:
```
Server=tcp:yourserver.database.windows.net,1433;Database=StudyTestSimulator;User Id=your_user;Password=your_password;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

## Database Setup

### 1. Install EF Core Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
```

### 2. Create Database Migration

```bash
cd StudyTestSimulator.Web
dotnet ef migrations add InitialCreate
```

### 3. Apply Migration to Database

```bash
dotnet ef database update
```

This will create the database and all required tables:
- TestCategories
- Questions
- Answers
- TestAttempts
- TestAttemptQuestions
- QuestionFlags

## Running the Application

### Development

```bash
cd StudyTestSimulator.Web
dotnet restore
dotnet build
dotnet run
```

Navigate to: `https://localhost:5001`

### Production Build

```bash
cd StudyTestSimulator.Web
dotnet publish -c Release -o ./publish
```

## Deployment to Windows Server

### Option 1: Run as Kestrel Service

1. **Install .NET 10 Runtime** on Windows Server

2. **Copy published files** to server (e.g., `C:\inetpub\StudyTestSimulator`)

3. **Update appsettings.json** with production values

4. **Create Windows Service** using NSSM or sc.exe:

```powershell
# Using NSSM (Non-Sucking Service Manager)
nssm install StudyTestSimulator "C:\Program Files\dotnet\dotnet.exe" "C:\inetpub\StudyTestSimulator\StudyTestSimulator.Web.dll"
nssm set StudyTestSimulator AppDirectory "C:\inetpub\StudyTestSimulator"
nssm set StudyTestSimulator AppEnvironmentExtra ASPNETCORE_ENVIRONMENT=Production
nssm start StudyTestSimulator
```

5. **Configure firewall** to allow traffic on port 5000/5001

6. **Setup reverse proxy** (recommended) using IIS or nginx

### Option 2: Host in IIS

1. Install **.NET 10 Hosting Bundle** for Windows

2. Create **IIS Application Pool**:
   - .NET CLR Version: No Managed Code
   - Managed Pipeline Mode: Integrated

3. Create **IIS Site**:
   - Point to publish folder
   - Bind to desired port/hostname
   - Set application pool

4. Ensure **Application Pool Identity** has access to:
   - Application files
   - SQL Server database

## Using the Application

### 1. Create Test Categories

1. Sign in with your Microsoft 365 account
2. Navigate to **Categories**
3. Click **Add Category**
4. Enter name and description
5. Click **Save**

### 2. Add Questions

#### Manual Entry
1. Navigate to **Manage Questions**
2. Select a category
3. Click **Add Question**
4. Enter question text, optional image (base64), and explanation
5. Add 4 answers (mark the correct one)
6. Click **Save**

#### JSON Import
1. Navigate to **Manage Questions**
2. Select a category
3. Click **Import from JSON**
4. Paste JSON data in the following format:

```json
{
  "questions": [
    {
      "questionText": "What is the capital of France?",
      "explanation": "Paris has been the capital of France since the 12th century.",
      "imageBase64": "data:image/png;base64,iVBORw0KG...", 
      "answers": [
        {
          "answerText": "London",
          "isCorrect": false,
          "explanation": "London is the capital of the United Kingdom."
        },
        {
          "answerText": "Paris",
          "isCorrect": true,
          "explanation": "Correct! Paris is the capital of France."
        },
        {
          "answerText": "Berlin",
          "isCorrect": false,
          "explanation": "Berlin is the capital of Germany."
        },
        {
          "answerText": "Madrid",
          "isCorrect": false,
          "explanation": "Madrid is the capital of Spain."
        }
      ]
    }
  ]
}
```

5. Click **Import**

### 3. Take a Test

1. Navigate to **Take Test**
2. Select a test category
3. Click **Start Test**
4. For each question:
   - Read the question and optional image
   - Select your answer
   - (Optional) Click **Check Answer** for immediate feedback
   - Click **Submit & Next** to advance
   - (Optional) Click **Flag Question** to mark for review
5. View your results summary
6. Click **Review Answers** to see detailed breakdown

### 4. Review History

1. Navigate to **Test History**
2. Filter by category (optional)
3. Click **Review** on any attempt to see:
   - Questions asked
   - Your answers
   - Correct answers
   - Explanations
   - Time spent per question

### 5. Manage Flagged Questions

1. Navigate to **Flagged Questions**
2. Review questions that users flagged during tests
3. Click **Mark as Resolved** after addressing issues

## Database Schema

### TestCategories
- Id, Name, Description, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate

### Questions
- Id, TestCategoryId, QuestionText, ImageBase64, Explanation, CreatedBy, CreatedDate, ModifiedBy, ModifiedDate, IsActive

### Answers
- Id, QuestionId, AnswerText, IsCorrect, Explanation, DisplayOrder

### TestAttempts
- Id, TestCategoryId, UserId, UserEmail, StartTime, EndTime, TotalQuestions, CorrectAnswers, PercentageScore, IsCompleted

### TestAttemptQuestions
- Id, TestAttemptId, QuestionId, SelectedAnswerId, IsCorrect, QuestionStartTime, QuestionEndTime, TimeSpentSeconds, QuestionOrder

### QuestionFlags
- Id, QuestionId, FlaggedBy, FlaggedByEmail, FlaggedDate, Comments, IsResolved, ResolvedBy, ResolvedDate

## Troubleshooting

### Authentication Issues

**Problem**: "Unauthorized" or "Access Denied" errors

**Solution**:
1. Verify Azure AD app registration settings
2. Check ClientId, TenantId, and ClientSecret in appsettings.json
3. Ensure redirect URIs match exactly (including https/http)
4. Verify API permissions are granted with admin consent

### Database Connection Issues

**Problem**: Cannot connect to SQL Server

**Solution**:
1. Verify connection string is correct
2. Check SQL Server is running
3. Ensure firewall allows connections
4. Test connection with SQL Server Management Studio
5. For Windows Auth, ensure app pool identity has access

### Migration Issues

**Problem**: "A network-related or instance-specific error"

**Solution**:
1. Ensure SQL Server Browser service is running
2. Enable TCP/IP protocol in SQL Server Configuration Manager
3. Verify server name format (localhost, .\SQLEXPRESS, etc.)

## Security Considerations

1. **Never commit appsettings.json with real secrets** - Use User Secrets or Azure Key Vault
2. **Use HTTPS in production** - Obtain and configure SSL certificate
3. **Keep dependencies updated** - Regular
ly update NuGet packages
4. **Validate user input** - The application uses EF Core which prevents SQL injection
5. **Backup database regularly** - Implement automated backup strategy

## License

This project is provided as-is for educational and organizational use.

## Support

For issues or questions, please contact your IT administrator or open an issue in the project repository.
