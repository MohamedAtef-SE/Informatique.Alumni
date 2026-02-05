import { api } from './api';

export interface AcademicCalendarItemDto {
    startDate: string;
    endDate?: string;
    eventName: string;
    description: string;
    semester: string | number;
}

export const calendarService = {
    getCalendar: async () => {
        const response = await api.get<AcademicCalendarItemDto[]>('/api/app/academic-calendar');
        return response.data;
    },

    downloadExcel: async () => {
        const response = await api.get('/api/app/academic-calendar/excel', {
            responseType: 'blob'
        });
        return response.data;
    }
};
