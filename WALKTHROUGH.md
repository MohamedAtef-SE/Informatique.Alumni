# Alumni Portal - System Status & Usage

## ✅ System Status
The Alumni Portal environment has been stabilized.
- **Frontend Port**: Enforced to `5173` (prevents port jumping/auth errors).
- **Authentication**: SSO Login/Logout fully functional.
- **Client Name**: Renamed to "Alumni Portal" (was "Console Test / Angular...").

## 🔑 Login Credentials
You can use the following accounts to test the system:

| Role | Username | Password |
|------|----------|----------|
| **Developer** | `devuser` | `Dev@123456` |
| **Admin** | `admin` | `1q2w3E*` |

## 📁 Project Structure
The project is organized with a clear separation of concerns:

- **`d:\Projects\Informatique.Alumni\client`**  
  👉 **Your React Application**. This is where you work. It uses `vite` and `react-oidc-context`.

- **`d:\Projects\Informatique.Alumni\src\Informatique.Alumni.HttpApi.Host`**  
  👉 **The Backend API & Identity Server**. This must be running for the app to work.

- **`d:\Projects\Informatique.Alumni\src\Informatique.Alumni.HttpApi.Client`**  
  👉 **Ignored**. This is a C# .NET Client SDK for other .NET apps. You do not need this for React.

## 🛠️ Usage Instructions

### 1. Start the Backend
Execute this in `src\Informatique.Alumni.HttpApi.Host`:
```powershell
dotnet run
```

### 2. Start the Frontend
Execute this in `client`:
```powershell
npm run dev
```
*Note: If port 5173 is busy, the command will now error instead of switching ports. Run `taskkill /F /IM node.exe` to free the port.*

### 3. Verification
- Access `http://localhost:5173`.
- Log in with `devuser`.
- Verify your name appears in the sidebar.
- Click "Sign Out" to return to the login screen.
