import type { ReactNode } from "react";
import { cn } from "../../utils/cn";

interface PageHeaderProps {
    title: string;
    description?: string;
    action?: ReactNode;
    className?: string;
}

export function PageHeader({ title, description, action, className }: PageHeaderProps) {
    return (
        <div className={cn("flex flex-col gap-4 md:flex-row md:items-center md:justify-between mb-6", className)}>
            <div className="space-y-1.5">
                <h1 className="text-2xl font-bold tracking-tight text-[var(--color-text-primary)]">{title}</h1>
                {description && (
                    <p className="text-sm text-[var(--color-text-secondary)]">
                        {description}
                    </p>
                )}
            </div>
            {action && (
                <div className="flex items-center gap-2">
                    {action}
                </div>
            )}
        </div>
    );
}
