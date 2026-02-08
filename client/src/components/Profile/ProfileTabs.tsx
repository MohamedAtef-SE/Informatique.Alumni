import { cn } from '../../utils/cn';
import { useTranslation } from 'react-i18next';

interface ProfileTabsProps {
    activeTab: string;
    onTabChange: (tab: string) => void;
}

const tabs = [
    { id: 'about', label: 'profile.tabs.about' },
    { id: 'experience', label: 'profile.tabs.experience' },
    { id: 'education', label: 'profile.tabs.education' },
    // { id: 'skills', label: 'Skills' }, // Future
];

const ProfileTabs = ({ activeTab, onTabChange }: ProfileTabsProps) => {
    const { t } = useTranslation();
    return (
        <div className="flex border-b border-gray-200 mb-6 overflow-x-auto">
            {tabs.map((tab) => (
                <button
                    key={tab.id}
                    onClick={() => onTabChange(tab.id)}
                    className={cn(
                        "px-6 py-3 text-sm font-medium transition-colors relative whitespace-nowrap",
                        activeTab === tab.id
                            ? "text-blue-600 after:absolute after:bottom-0 after:left-0 after:right-0 after:h-0.5 after:bg-blue-600"
                            : "text-slate-500 hover:text-slate-700"
                    )}
                >
                    {t(tab.label)}
                </button>
            ))}
        </div>
    );
};

export default ProfileTabs;
