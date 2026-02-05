# Fixes Summary - Login & Frontend Stability

The following issues haven been resolved to ensure a stable login flow and correct data display.

## 1. Login "No matching state found" Error
**Issue**: Users were seeing "Login Error: No matching state found in storage" after authenticating.
**Cause**: The OIDC client was using `sessionStorage` (default) for authorization state, which was getting lost/cleared during the redirect flow or tab switches.
**Fix**: Updated `client/src/services/auth.ts` to explicitly use `localStorage` for the state store.

```typescript
// auth.ts
stateStore: new WebStorageStateStore({ store: window.localStorage }),
```

## 2. Invalid Username/Password for 'devuser'
**Issue**: The verification `devuser` created by the seeder had a password mismatch or failed to be created correctly.
**Fix**: Updated `DevDataSeederContributor.cs` to distinctively remove any existing password and set the correct one (`Dev@123456`) using `RemovePasswordAsync` and `AddPasswordAsync` instead of relying on token-based resets.

## 3. "Unexpected token <" Crash & API Redirects
**Issue**: Frontend pages crashed with JSON parsing errors because the API was returning HTML pages (login screen) instead of JSON errors for unauthorized requests.
**Fix**: Added `X-Requested-With: XMLHttpRequest` header to `client/src/services/api.ts`. This instructs the ABP backend to return `401 Unauthorized` JSON responses instead of `302 Redirect` to HTML pages.

## 4. "John Doe" Mock Data
**Issue**: The sidebar displayed confusing hardcoded data ("John Doe / Class of 2024").
**Fix**: Updated `Sidebar.tsx` to read the authenticated user's profile from the OIDC `useAuth` hook.

## 5. Page Crashes (Directory/Gallery)
**Issue**: Pages were crashing with "Cannot read properties of undefined (reading 'map')" when API calls failed or returned null data.
**Fix**: Added error handling (`isError`) and defensive null checks (`data?.items ?? []`) to `Directory.tsx` and `Gallery.tsx`.

---

## Verification Steps

1. **Clear Browser State**: Close existing tabs or clear Local Storage to ensure a fresh start.
2. **Access App**: Go to `http://localhost:5173`.
3. **Login**: 
   - User: `devuser`
   - Pass: `Dev@123456`
4. **Verify**:
   - Sidebar shows "devuser"
   - Sign Out works correctly
   - Directory and Gallery pages load without crashing (even if empty)
