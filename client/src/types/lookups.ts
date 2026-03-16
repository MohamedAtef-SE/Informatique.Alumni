export interface CurrencyDto {
    id: string;
    code: string;
    name: string;
    symbol: string;
    flagEmoji: string;
    exchangeRateFromUSD: number;
    isBase: boolean;
    lastSyncedAt?: string;
}

export interface LookupItemDto {
    value: string;
    label: string;
}
