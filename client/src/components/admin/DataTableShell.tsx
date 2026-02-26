import type { ReactNode } from "react";
import { cn } from "../../utils/cn";
import { Input } from "../ui/Input";
import { Button } from "../ui/Button";
import { Search } from "lucide-react";

interface DataTableShellProps {
    children: ReactNode;
    className?: string;
    searchPlaceholder?: string;
    onSearch?: (value: string) => void;
    pagination?: {
        currentPage: number;
        totalPages: number;
        onPageChange: (page: number) => void;
    };
}

export function DataTableShell({
    children,
    className,
    searchPlaceholder = "Search...",
    onSearch,
    pagination
}: DataTableShellProps) {
    return (
        <div className={cn("space-y-4", className)}>
            {onSearch && (
                <div className="flex items-center justify-between">
                    <div className="relative w-full max-w-sm">
                        <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-[var(--color-text-secondary)]" />
                        <Input
                            type="search"
                            placeholder={searchPlaceholder}
                            className="pl-9 bg-white"
                            onChange={(e) => onSearch(e.target.value)}
                        />
                    </div>
                </div>
            )}

            <div className="rounded-md border border-[var(--color-border)] bg-white overflow-hidden">
                {children}
            </div>

            {pagination && (
                <div className="flex items-center justify-end space-x-2 py-4">
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => pagination.onPageChange(pagination.currentPage - 1)}
                        disabled={pagination.currentPage <= 1}
                    >
                        Previous
                    </Button>
                    <div className="text-sm text-[var(--color-text-secondary)]">
                        Page {pagination.currentPage} of {pagination.totalPages}
                    </div>
                    <Button
                        variant="outline"
                        size="sm"
                        onClick={() => pagination.onPageChange(pagination.currentPage + 1)}
                        disabled={pagination.currentPage >= pagination.totalPages}
                    >
                        Next
                    </Button>
                </div>
            )}
        </div>
    );
}
