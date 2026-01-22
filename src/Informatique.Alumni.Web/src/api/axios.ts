import axios from 'axios';
import { User } from 'oidc-client-ts';

// Create Axios Instance
export const api = axios.create({
    baseURL: '/api', // Proxied to Backend
    timeout: 30000,
    headers: {
        'Content-Type': 'application/json',
    },
});

/**
 * Access Token Interceptor
 * 
 * We use `oidc-client-ts` storage mechanism to find the token.
 * Since we are using `react-oidc-context`, we can grab the user from Session/LocalStorage 
 * depending on config.
 */
api.interceptors.request.use(
    (config) => {
        // Attempt to get token from storage (oidc-client-ts default key pattern)
        // Key format: `oidc.user:<authority>:<client_id>`
        // For now, we rely on a helper or basic check. 
        // In a real app, passing the user from Context to the API wrapper is safer, 
        // but interceptors work if storage is standard.

        // Fallback: Check for custom storage key if your app uses one.
        // Assuming standard OIDC Client default `sessionStorage` or `localStorage`.

        // We will implementing a proper token injection via a wrapper hook or similar later.
        // For now, let's look for the standard OIDC key pattern in sessionStorage.
        const oidcStorage = sessionStorage.getItem(`oidc.user:https://localhost:44386:Alumni_Web`);
        if (oidcStorage) {
            const user = User.fromStorageString(oidcStorage);
            if (user?.access_token) {
                config.headers.Authorization = `Bearer ${user.access_token}`;
            }
        }

        return config;
    },
    (error) => Promise.reject(error)
);

/**
 * Global Error Handling
 */
api.interceptors.response.use(
    (response) => response,
    (error) => {
        // Handle 401/403
        if (error.response?.status === 401) {
            // Redirect to Login? Or let AuthProvider handle it?
            console.warn('Unauthorized - Token might be expired');
        }
        return Promise.reject(error);
    }
);
