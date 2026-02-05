import { motion, AnimatePresence } from 'framer-motion';
import { useLoaderStore } from '../../stores/useLoaderStore';
import { Loader2 } from 'lucide-react';

const GlobalLoader = () => {
    const { isLoading } = useLoaderStore();

    return (
        <AnimatePresence>
            {isLoading && (
                <motion.div
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    exit={{ opacity: 0 }}
                    transition={{ duration: 0.3 }}
                    className="fixed inset-0 z-[60] flex items-center justify-center bg-black/60 backdrop-blur-md"
                >
                    <div className="flex flex-col items-center gap-4">
                        <div className="relative">
                            <div className="absolute inset-0 rounded-full bg-accent/20 blur-xl animate-pulse"></div>
                            <div className="relative bg-slate-900/80 border border-white/10 p-6 rounded-2xl shadow-neon flex items-center justify-center">
                                <Loader2 className="w-10 h-10 text-accent animate-spin" />
                            </div>
                        </div>
                        <motion.p
                            initial={{ opacity: 0, y: 10 }}
                            animate={{ opacity: 1, y: 0 }}
                            className="text-white font-medium tracking-wider"
                        >
                            Loading<span className="animate-pulse">...</span>
                        </motion.p>
                    </div>
                </motion.div>
            )}
        </AnimatePresence>
    );
};

export default GlobalLoader;
