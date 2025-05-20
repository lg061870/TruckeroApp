## 📌 Tenant and Subscription

- **Primary Tenant (TruckeroCorp)**
    - Tenant ID: `7c71e621-d2aa-4784-ab69-6b16b19a47df`
    - Domain: `lg061870gmail.onmicrosoft.com`
- **Azure Sponsorship Subscription**
    - Name: `Microsoft Azure Sponsorship`
    - ID: `64322646-a81d-47d3-841c-f86e704acc27`
- **B2C Tenant (TruckeroAppAuth)**
    - Tenant ID: `b4e24d9c-c7a7-4457-a784-ae13a6d782b8`
    - Domain: `truckeroappauth.onmicrosoft.com`

---

## 🔐 Azure AD B2C Configuration

- App Registration: `TruckeroApp`
    - App ID: `67970fb3-f955-421c-b6b8-f475546c617e`
    - Reply URL: `https://truckero-api-dev.azurewebsites.net/signin-oidc`
    - Authority: `https://truckeroappauth.b2clogin.com/truckeroappauth.onmicrosoft.com/B2C_1_signupsignin`
- Custom Roles (from `roles.json`):
    - `Consumer` — `b68fae1e-0111-4ac3-a02e-111111111111`
    - `Driver` — `c7c27f6a-2222-46bb-8b7d-222222222222`
    - `StoreClerk` — `d0a8b678-3333-40e1-901f-333333333333`
    - `Admin` — `ae56dd90-4444-4df2-b1d4-444444444444`

---

## 🔐 Azure Key Vault

- Name: `truckero-keyvault-dev`
- URI: `https://truckero-keyvault-dev.vault.azure.net/`
- Tenant ID: `7c71e621-d2aa-4784-ab69-6b16b19a47df`
- Resource Group: `truckero-core-dev`
- Stored Secrets:
    - `B2C-Client-ID`: `67970fb3-f955-421c-b6b8-f475546c617e`
    - `B2C-Client-Secret`: *(secure)*
    - `B2C-Tenant`: `truckeroappauth.onmicrosoft.com`

---

## 🌐 App Service Plan

- Name: `truckero-app-plan-dev`
- SKU: `B1 (Basic Tier)` — ~$13/month
- OS: Linux
- Region: `East US`
- Subscription: `Microsoft Azure Sponsorship`

---

## 📁 Resource Groups

| Environment | Resource Group | Region | Status |
| --- | --- | --- | --- |
| Dev | truckero-core-dev | East US | ✅ |
| Test | truckero-core-test | East US | ⏳ |
| Prod | truckero-core-prod | East US | ⏳ |

---

## ⚙️ Identity Integration in ASP.NET Core

App reads configuration securely from Azure Key Vault using `SecretClient`:

```csharp
csharp
CopyEdit
var secretClient = new SecretClient(
    new Uri("https://truckero-keyvault-dev.vault.azure.net/"),
    new DefaultAzureCredential());

string clientId = secretClient.GetSecret("B2C-Client-ID").Value.Value;
string clientSecret = secretClient.GetSecret("B2C-Client-Secret").Value.Value;

```

Used in:

```csharp
csharp
CopyEdit
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        options.ClientId = clientId;
        options.ClientSecret = clientSecret;
        options.Authority = "https://truckeroappauth.b2clogin.com/truckeroappauth.onmicrosoft.com/B2C_1_signupsignin";
        options.ResponseType = "code";
        options.SaveTokens = true;
    });

```

---

## 🚀 API Status

✅ `dotnet run` is ready after NuGet updates:

- `Microsoft.Identity.Web`
- `Azure.Identity`
- `Azure.Security.KeyVault.Secrets`

✅ Endpoint `/weatherforecast` is now **protected via Azure AD B2C**.

---

## ✅ Summary of Registered Components

| Component | Identifier/Name |
| --- | --- |
| B2C Tenant | `truckeroappauth.onmicrosoft.com` |
| B2C App ID | `67970fb3-f955-421c-b6b8-f475546c617e` |
| Key Vault URI | `https://truckero-keyvault-dev.vault.azure.net/` |
| Client Secret | *(stored securely in Key Vault)* |
| App Service Plan | `truckero-app-plan-dev` |
| Subscription | `Microsoft Azure Sponsorship (6432...acc27)` |
|  |  |

---

## 🔜 Next Actions

| Step | Tool |
| --- | --- |
| ✅ Create Dev RG and App Infra | Azure CLI |
| ✅ Configure identity flow with B2C | Azure CLI + .NET |
| ⏳ Configure DevOps CI/CD | Azure DevOps or GitHub |
| ⏳ Create Test + Prod Resource Groups | Azure CLI |
| ⏳ DNS mapping and deployment slots | Azure Porta |

CLI ACCESS KEY: XHXs69rH1p4rX2EQN6yiIBJD7WkWo4JsA6c9IvBZWYlQd4cj9iizJQQJ99BDACAAAAAAAAAAAAASAZDO3h8T