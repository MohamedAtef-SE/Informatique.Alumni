import { Bell, Search, Menu } from 'lucide-react';
import { cn } from '../../utils/cn';

interface HeaderProps {
    onMenuClick: () => void;
    isSidebarOpen: boolean;
}

const Header = ({ onMenuClick, isSidebarOpen }: HeaderProps) => {

    return (
        <header className={cn(
            "h-20 fixed top-0 right-0 z-40 px-6 flex items-center justify-between",
            "bg-slate-900/80 backdrop-blur-lg border-b border-white/5",
            "transition-all duration-300 ease-in-out",
            isSidebarOpen ? "left-0 lg:left-[280px]" : "left-0"
        )}>

            <div className="flex items-center gap-6 flex-1">
                {/* Mobile Menu Button */}
                <button
                    onClick={onMenuClick}
                    className="p-2 text-slate-400 hover:text-white hover:bg-white/5 rounded-lg transition-colors lg:hidden"
                >
                    <Menu className="w-6 h-6" />
                </button>

                {/* Search Bar */}
                <div className="relative group hidden sm:block max-w-md w-full">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-slate-500 group-focus-within:text-accent transition-colors" />
                    <input
                        type="text"
                        placeholder="Search for alumni, events, or jobs..."
                        className="w-full bg-slate-950/50 border border-white/10 rounded-xl pl-10 pr-4 py-2.5 text-sm text-white focus:outline-none focus:border-accent/50 focus:ring-1 focus:ring-accent/50 transition-all placeholder:text-slate-600"
                    />
                </div>
            </div>

            {/* Actions */}
            <div className="flex items-center gap-4">
                <button className="relative p-2.5 text-slate-400 hover:text-accent transition-colors hover:bg-white/5 rounded-full group">
                    <Bell className="w-5 h-5 group-hover:animate-swing" />
                    <span className="absolute top-2 right-2 w-2 h-2 rounded-full bg-red-500 border-2 border-slate-900 animate-pulse"></span>
                </button>
            </div>
        </header>
    );
};

export default Header;

