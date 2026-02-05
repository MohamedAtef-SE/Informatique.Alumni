import React from 'react';
import { AlertTriangle, X } from 'lucide-react';

interface ErrorModalProps {
    isOpen: boolean;
    title: string;
    message: string;
    onClose: () => void;
}

const ErrorModal: React.FC<ErrorModalProps> = ({ isOpen, title, message, onClose }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
            <div className="bg-[#1a1f2e] border border-white/10 rounded-xl p-6 w-full max-w-sm space-y-4 shadow-2xl animate-fade-in relative">
                {/* Close Button */}
                <button
                    onClick={onClose}
                    className="absolute top-4 right-4 text-gray-400 hover:text-white transition-colors"
                >
                    <X className="w-5 h-5" />
                </button>

                <div className="flex flex-col items-center text-center space-y-3 pt-2">
                    <div className="w-12 h-12 bg-amber-500/10 rounded-full flex items-center justify-center">
                        <AlertTriangle className="w-6 h-6 text-amber-500" />
                    </div>
                    <h3 className="text-xl font-bold text-white max-w-[90%]">{title}</h3>
                    <p className="text-gray-400 text-sm whitespace-pre-line leading-relaxed">
                        {message}
                    </p>
                </div>

                <button
                    onClick={onClose}
                    className="w-full btn-primary flex items-center justify-center gap-2"
                >
                    OK, Got it
                </button>
            </div>
        </div>
    );
};

export default ErrorModal;
