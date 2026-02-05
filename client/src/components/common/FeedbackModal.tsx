import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { CheckCircle2, AlertTriangle, Info, X } from 'lucide-react';
import { cn } from '../../utils/cn';

type ModalVariant = 'success' | 'warning' | 'info';

interface FeedbackModalProps {
    isOpen: boolean;
    variant: ModalVariant;
    title: string;
    message: string;
    onClose: () => void;
    actionLabel?: string;
}

const variantStyles: Record<ModalVariant, { icon: React.ElementType; iconBg: string; iconColor: string }> = {
    success: { icon: CheckCircle2, iconBg: 'bg-emerald-500/10', iconColor: 'text-emerald-500' },
    warning: { icon: AlertTriangle, iconBg: 'bg-amber-500/10', iconColor: 'text-amber-500' },
    info: { icon: Info, iconBg: 'bg-blue-500/10', iconColor: 'text-blue-500' },
};

const FeedbackModal: React.FC<FeedbackModalProps> = ({
    isOpen,
    variant,
    title,
    message,
    onClose,
    actionLabel = 'OK, Got it',
}) => {
    const { icon: Icon, iconBg, iconColor } = variantStyles[variant];

    return (
        <AnimatePresence>
            {isOpen && (
                <motion.div
                    className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/80 backdrop-blur-sm p-4"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    transition={{ duration: 0.2 }}
                    onClick={onClose}
                >
                    <motion.div
                        className="bg-white border border-slate-200 rounded-xl p-6 w-full max-w-sm space-y-4 shadow-2xl relative"
                        initial={{ scale: 0.9, opacity: 0, y: 20 }}
                        animate={{ scale: 1, opacity: 1, y: 0 }}
                        exit={{ scale: 0.9, opacity: 0, y: 20 }}
                        transition={{ type: 'spring', damping: 25, stiffness: 300 }}
                        onClick={(e) => e.stopPropagation()}
                    >
                        {/* Close Button */}
                        <motion.button
                            onClick={onClose}
                            className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 transition-colors"
                            whileHover={{ scale: 1.1 }}
                            whileTap={{ scale: 0.9 }}
                        >
                            <X className="w-5 h-5" />
                        </motion.button>

                        <div className="flex flex-col items-center text-center space-y-3 pt-2">
                            <motion.div
                                className={cn('w-14 h-14 rounded-full flex items-center justify-center', iconBg)}
                                initial={{ scale: 0 }}
                                animate={{ scale: 1 }}
                                transition={{ type: 'spring', damping: 15, stiffness: 200, delay: 0.1 }}
                            >
                                <Icon className={cn('w-7 h-7', iconColor)} />
                            </motion.div>
                            <h3 className="text-xl font-bold text-[var(--color-text-primary)]">{title}</h3>
                            <p className="text-[var(--color-text-secondary)] text-sm whitespace-pre-line leading-relaxed">
                                {message}
                            </p>
                        </div>

                        <motion.button
                            onClick={onClose}
                            className={cn(
                                'w-full py-3 px-4 rounded-lg font-medium transition-colors',
                                variant === 'success' && 'bg-emerald-500 hover:bg-emerald-600 text-white',
                                variant === 'warning' && 'bg-amber-500 hover:bg-amber-600 text-white',
                                variant === 'info' && 'bg-blue-500 hover:bg-blue-600 text-white'
                            )}
                            whileHover={{ scale: 1.02 }}
                            whileTap={{ scale: 0.98 }}
                        >
                            {actionLabel}
                        </motion.button>
                    </motion.div>
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default FeedbackModal;
