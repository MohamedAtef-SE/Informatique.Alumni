import React from 'react';
import { useSessionStore } from '../../stores/useSessionStore';
import { LogOut, AlertTriangle } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { userManager } from '../../services/auth';

const SessionExpiredModal: React.FC = () => {
    const { isSessionExpired, hideSessionExpired } = useSessionStore();
    const navigate = useNavigate();

    const handleLogin = async () => {
        hideSessionExpired();
        await userManager.removeUser(); // Clear local OIDC storage
        navigate('/auth/login');
    };

    if (!isSessionExpired) return null;

    return (
        <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-black/80 backdrop-blur-sm p-4">
            <div className="bg-[#1a1f2e] border border-white/10 rounded-xl p-6 w-full max-w-sm space-y-4 shadow-2xl animate-fade-in">
                <div className="flex flex-col items-center text-center space-y-3">
                    <div className="w-12 h-12 bg-red-500/10 rounded-full flex items-center justify-center">
                        <AlertTriangle className="w-6 h-6 text-red-500" />
                    </div>
                    <h3 className="text-xl font-bold text-white">Session Expired</h3>
                    <p className="text-gray-400 text-sm">
                        Your session has expired. Please log in again to continue using the application.
                    </p>
                </div>

                <button
                    onClick={handleLogin}
                    className="w-full btn-primary flex items-center justify-center gap-2"
                >
                    <LogOut className="w-4 h-4" /> Log In Again
                </button>
            </div>
        </div>
    );
};

export default SessionExpiredModal;
