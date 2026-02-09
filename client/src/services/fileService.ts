import { api } from './api';

export const fileService = {
    upload: async (file: File) => {
        const formData = new FormData();
        formData.append('file', file);

        // Upload to the new generic file upload endpoint
        // URL based on FileAppService implementation: /api/app/file/upload (if using auto-controller)
        // Check FileAppService.cs: it implements IFileAppService.
        // ABP auto-api-controller usually map to /api/app/file
        // Method name UploadAsync -> POST /api/app/file/upload

        const response = await api.post<string>('/api/app/file/upload', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });

        return response.data;
    }
};
