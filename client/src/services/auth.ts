import { UserManager, WebStorageStateStore } from 'oidc-client-ts';
import type { AuthProviderProps } from 'react-oidc-context';

export const oidcConfig: AuthProviderProps = {
    authority: 'https://localhost:44386',
    client_id: 'Alumni_App',
    redirect_uri: 'http://localhost:5173/auth/callback',
    post_logout_redirect_uri: 'http://localhost:5173',
    response_type: 'code',
    scope: 'openid profile email roles Alumni offline_access',
    userStore: new WebStorageStateStore({ store: window.localStorage }),
    stateStore: new WebStorageStateStore({ store: window.localStorage }), // Ensure state persists across tabs/redirects
    automaticSilentRenew: true,
    loadUserInfo: true,
    onSigninCallback: (_user) => {
        // Navigate to dashboard after login
        window.history.replaceState({}, document.title, window.location.pathname);
        window.location.href = '/portal';
    },
};

export const userManager = new UserManager({
    ...oidcConfig,
    redirect_uri: 'http://localhost:5173/auth/callback', // Type mismatch fix if any
} as any);
