import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { CurrencyDto } from '../types/lookups';

interface CurrencyState {
    /** The currently selected currency (from backend lookup table) */
    selectedCurrency: CurrencyDto | null;
    /** Set active currency (called when user picks from dropdown) */
    setSelectedCurrency: (currency: CurrencyDto) => void;
}

export const useCurrencyStore = create<CurrencyState>()(
    persist(
        (set) => ({
            selectedCurrency: null,
            setSelectedCurrency: (currency) => set({ selectedCurrency: currency }),
        }),
        {
            name: 'alumni-currency-preference',
            // Only persist the code, not the full object — we'll re-match on load
            partialize: (state) => ({
                selectedCurrency: state.selectedCurrency
                    ? { code: state.selectedCurrency.code } as CurrencyDto
                    : null
            }),
        }
    )
);

/**
 * Convert a USD amount to the currently selected currency.
 * Falls back to USD if no currency selected.
 */
export function convertFromUSD(usdAmount: number, currency: CurrencyDto | null): number {
    if (!currency) return usdAmount;
    return usdAmount * currency.exchangeRateFromUSD;
}

/**
 * Format a USD amount as a localised currency string using the selected currency.
 * Uses native Intl.NumberFormat — no external libraries.
 */
export function formatCurrency(usdAmount: number, currency: CurrencyDto | null): string {
    const resolved = currency ?? { code: 'USD', symbol: '$', exchangeRateFromUSD: 1 } as CurrencyDto;
    const converted = usdAmount * resolved.exchangeRateFromUSD;

    try {
        return new Intl.NumberFormat(undefined, {
            style: 'currency',
            currency: resolved.code,
            minimumFractionDigits: 0,
            maximumFractionDigits: 2,
        }).format(converted);
    } catch {
        // Fallback for currencies Intl may not know
        return `${resolved.symbol}${converted.toLocaleString(undefined, { maximumFractionDigits: 2 })}`;
    }
}

/**
 * Try to pick the closest currency from the backend list based on the browser locale.
 * Returns null if no match found (caller should fall back to USD).
 */
export function detectCurrencyFromLocale(currencies: CurrencyDto[]): CurrencyDto | null {
    const locale = navigator.language || 'en-US';
    const regionCode = locale.split('-')[1]?.toUpperCase();
    if (!regionCode) return null;

    // Map of region code → ISO 4217 currency code
    const REGION_CURRENCY: Record<string, string> = {
        EG: 'EGP', US: 'USD', GB: 'GBP', SA: 'SAR', AE: 'AED', KW: 'KWD',
        DE: 'EUR', FR: 'EUR', IT: 'EUR', ES: 'EUR', PT: 'EUR', NL: 'EUR',
        CA: 'CAD', AU: 'AUD', JP: 'JPY', CN: 'CNY', IN: 'INR', BR: 'BRL',
        MX: 'MXN', TR: 'TRY', QA: 'QAR', BH: 'BHD', OM: 'OMR',
    };

    const targetCode = REGION_CURRENCY[regionCode];
    if (!targetCode) return null;
    return currencies.find(c => c.code === targetCode) ?? null;
}
