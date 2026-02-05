import { create } from 'zustand';

interface LoaderState {
    isLoading: boolean;
    activeRequests: number;
    startTime: number | null;
    showLoader: () => void;
    hideLoader: () => void;
}

const MIN_LOADING_TIME = 1000; // 1 second minimum

export const useLoaderStore = create<LoaderState>((set, get) => ({
    isLoading: false,
    activeRequests: 0,
    startTime: null,

    showLoader: () => {
        const { activeRequests } = get();
        if (activeRequests === 0) {
            set({ isLoading: true, startTime: Date.now(), activeRequests: 1 });
        } else {
            set((state) => ({ activeRequests: state.activeRequests + 1 }));
        }
    },

    hideLoader: () => {
        const { activeRequests, startTime } = get();

        if (activeRequests <= 1) {
            // Last request finishing
            const elapsedTime = Date.now() - (startTime || 0);
            const remainingTime = Math.max(0, MIN_LOADING_TIME - elapsedTime);

            if (remainingTime > 0) {
                setTimeout(() => {
                    set({ isLoading: false, activeRequests: 0, startTime: null });
                }, remainingTime);
            } else {
                set({ isLoading: false, activeRequests: 0, startTime: null });
            }
        } else {
            set((state) => ({ activeRequests: state.activeRequests - 1 }));
        }
    }
}));
