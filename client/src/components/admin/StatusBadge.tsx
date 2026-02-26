import { cva, type VariantProps } from "class-variance-authority";
import { cn } from "../../utils/cn";

const badgeVariants = cva(
    "inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-semibold transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
    {
        variants: {
            variant: {
                default: "bg-slate-900 text-slate-100 hover:bg-slate-900/80",
                secondary: "bg-slate-100 text-slate-900 hover:bg-slate-100/80",
                destructive: "bg-red-500 text-slate-50 hover:bg-red-500/80",
                success: "bg-green-500 text-slate-50 hover:bg-green-500/80",
                warning: "bg-yellow-500 text-white hover:bg-yellow-500/80",
                info: "bg-blue-500 text-slate-50 hover:bg-blue-500/80",
                outline: "text-slate-950 border border-slate-200 hover:bg-slate-100",
            },
        },
        defaultVariants: {
            variant: "default",
        },
    }
);

export interface StatusBadgeProps
    extends React.HTMLAttributes<HTMLDivElement>,
    VariantProps<typeof badgeVariants> { }

function StatusBadge({ className, variant, ...props }: StatusBadgeProps) {
    return (
        <div className={cn(badgeVariants({ variant }), className)} {...props} />
    );
}

export { StatusBadge, badgeVariants };
