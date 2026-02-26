export interface DistributionItem {
    label: string;
    count: number;
}

export interface MonthlyMetric {
    month: string;
    count: number;
}

export interface ActivityItem {
    time: string;
    description: string;
    type: string;
}

export interface DashboardStatsDto {
    totalAlumni: number;
    pendingAlumni: number;
    activeAlumni: number;
    rejectedAlumni: number;
    bannedAlumni: number;
    activeJobs: number;
    upcomingEvents: number;
    pendingGuidanceRequests: number;
    pendingSyndicateRequests: number;
    totalRevenue: number;
    alumniByCollege: DistributionItem[];
    topEmployers: DistributionItem[];
    topLocations: DistributionItem[];
    monthlyRegistrations: MonthlyMetric[];
    recentActivities: ActivityItem[];
}
