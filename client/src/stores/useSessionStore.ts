import { create } from 'zustand';

interface SessionStore {
    isSessionExpired: boolean;
    showSessionExpired: () => void;
    hideSessionExpired: () => void;
}

export const useSessionStore = create<SessionStore>((set) => ({
    isSessionExpired: false,
    showSessionExpired: () => set({ isSessionExpired: true }),
    hideSessionExpired: () => set({ isSessionExpired: false }),
}));
