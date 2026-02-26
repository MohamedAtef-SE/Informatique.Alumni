import axios from 'axios';
import { useLoaderStore } from '../stores/useLoaderStore';
import { useSessionStore } from '../stores/useSessionStore';
import i18n from '../i18n';

// Default to localhost if environment variable is not set
export const BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:44386';

// OIDC Configuration (must match auth.ts)
const OIDC_AUTHORITY = 'https://localhost:44386';
const OIDC_CLIENT_ID = 'Alumni_App';

/**
 * Get the access token from OIDC storage
 * oidc-client-ts stores user data in localStorage with key: oidc.user:{authority}:{client_id}
 */
const getAccessToken = (): string | null => {
    try {
        const oidcStorageKey = `oidc.user:${OIDC_AUTHORITY}:${OIDC_CLIENT_ID}`;
        const oidcStorage = localStorage.getItem(oidcStorageKey);
        if (oidcStorage) {
            const user = JSON.parse(oidcStorage);
            return user?.access_token || null;
        }
    } catch (e) {
        console.error('Error reading OIDC token from storage:', e);
    }
    return null;
};

export const api = axios.create({
    baseURL: BASE_URL,
    headers: {
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest', // Forces 401 instead of 302 for API calls
    },
    withCredentials: true, // Important for cookies
    xsrfCookieName: 'XSRF-TOKEN',
    xsrfHeaderName: 'RequestVerificationToken',
});

// Request Interceptor for Auth & Loader
api.interceptors.request.use(
    (config) => {
        // Show loader for all requests unless explicitly skipped (e.g., background polling or blob downloads)
        if (config.responseType !== 'blob' && !(config as any).skipLoader) {
            useLoaderStore.getState().showLoader();
        }

        const token = getAccessToken();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }

        // Pass the current language to the backend
        if (i18n.language) {
            config.headers['Accept-Language'] = i18n.language;
        }

        return config;
    },
    (error) => {
        if (error.config?.responseType !== 'blob' && !(error.config as any)?.skipLoader) {
            useLoaderStore.getState().hideLoader();
        }
        return Promise.reject(error);
    }
);

// Response Interceptor for Errors & Loader
api.interceptors.response.use(
    (response) => {
        if (response.config.responseType !== 'blob' && !(response.config as any).skipLoader) {
            useLoaderStore.getState().hideLoader();
        }
        return response;
    },
    (error) => {
        if (error.config?.responseType !== 'blob' && !(error.config as any)?.skipLoader) {
            useLoaderStore.getState().hideLoader();
        }
        // Handle 401/403
        if (error.response?.status === 401) {
            console.warn('Unauthorized - Token may be expired or invalid');
            useSessionStore.getState().showSessionExpired();
        }
        return Promise.reject(error);
    }
);

