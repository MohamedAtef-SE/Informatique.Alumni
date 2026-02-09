import { Search, MapPin, Stethoscope } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Input } from "../../ui/Input";
import { Button } from "../../ui/Button";

interface PartnerFilterBarProps {
    filterText: string;
    setFilterText: (text: string) => void;
    category: string;
    setCategory: (cat: string) => void;
    city: string;
    setCity: (city: string) => void;
    onClear: () => void;
}

export const PartnerFilterBar = ({
    filterText, setFilterText,
    category, setCategory,
    city, setCity,
    onClear
}: PartnerFilterBarProps) => {
    const { t } = useTranslation();

    return (
        <div className="bg-[var(--color-bg-card)] p-4 rounded-xl border border-[var(--color-border)] shadow-sm space-y-4 md:space-y-0 md:flex md:items-center md:gap-4">
            {/* Search Text */}
            <div className="relative flex-1">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-[var(--color-text-muted)]" />
                <Input
                    placeholder={t('services.health.searchPlaceholder', 'Ischemia, Dr. Ahmed...')}
                    value={filterText}
                    onChange={(e: React.ChangeEvent<HTMLInputElement>) => setFilterText(e.target.value)}
                    className="pl-9 bg-[var(--color-bg-secondary)] text-[var(--color-text-primary)] border-none focus-visible:ring-1"
                />
            </div>

            {/* Category Filter */}
            <div className="w-full md:w-48 relative">
                <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none text-[var(--color-text-secondary)]">
                    <Stethoscope className="w-4 h-4" />
                </div>
                <select
                    className="w-full pl-9 pr-8 py-2 bg-[var(--color-bg-secondary)] border-none rounded-lg text-sm text-[var(--color-text-primary)] focus:ring-1 focus:ring-[var(--color-accent)] outline-none appearance-none cursor-pointer"
                    value={category}
                    onChange={(e) => setCategory(e.target.value)}
                >
                    <option value="">{t('services.health.filterCategory', 'All Categories')}</option>
                    <option value="Pharmacy">Pharmacy</option>
                    <option value="Hospital">Hospital</option>
                    <option value="Lab">Lab</option>
                    <option value="Dental">Dental</option>
                    <option value="Optical">Optical</option>
                </select>
            </div>

            {/* City Filter */}
            <div className="w-full md:w-40 relative">
                <div className="absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none text-[var(--color-text-secondary)]">
                    <MapPin className="w-4 h-4" />
                </div>
                <select
                    className="w-full pl-9 pr-8 py-2 bg-[var(--color-bg-secondary)] border-none rounded-lg text-sm text-[var(--color-text-primary)] focus:ring-1 focus:ring-[var(--color-accent)] outline-none appearance-none cursor-pointer"
                    value={city}
                    onChange={(e) => setCity(e.target.value)}
                >
                    <option value="">{t('services.health.filterCity', 'All Cities')}</option>
                    <option value="Cairo">Cairo</option>
                    <option value="Giza">Giza</option>
                    <option value="Alexandria">Alexandria</option>
                </select>
            </div>

            {/* Clear Button */}
            {(filterText || category || city) && (
                <Button variant="ghost" size="sm" onClick={onClear} className="text-[var(--color-text-muted)] hover:text-[var(--color-error)]">
                    Clear
                </Button>
            )}
        </div>
    );
};
