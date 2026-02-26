import { api } from './api';
import type { PagedResultDto, PagedAndSortedResultRequestDto } from '../types/common';
import type { AssociationRequestDto, MembershipRequestFilterDto, UpdateStatusDto } from '../types/membership';
import type { EventListDto, CreateEventDto, UpdateEventDto } from '../types/events';
import type { BlogPostDto, CreateBlogPostDto, UpdateBlogPostDto } from '../types/news';
import type { CareerServiceDto, CreateCareerServiceDto } from '../types/career';
import type { AcademicGrantDto, CommercialDiscountDto, CreateAcademicGrantDto, CreateCommercialDiscountDto } from '../types/benefits';
import type { GalleryAlbumDto, CreateGalleryAlbumDto } from '../types/gallery';
import type { UpdateCertificateStatusDto } from '../types/certificates';
import type { SyndicateStatus } from '../types/syndicates';
import type { MedicalPartnerDto, CreateMedicalPartnerDto, UpdateMedicalPartnerDto } from '../types/health';
import type { BranchDto, CreateUpdateBranchDto } from '../types/organization';
import type { AlumniTripDto, CreateTripDto } from '../types/trips';
import type { DashboardStatsDto } from '../types/admin';

export const adminService = {
    // Membership Management
    getRequests: async (input: MembershipRequestFilterDto) => {
        const response = await api.get<PagedResultDto<AssociationRequestDto>>('/api/app/membership', {
            params: input
        });
        return response.data;
    },

    approveRequest: async (id: string) => {
        const response = await api.post<AssociationRequestDto>(`/api/app/membership/${id}/approve`);
        return response.data;
    },

    rejectRequest: async (id: string, reason: string) => {
        await api.post(`/api/app/membership/${id}/reject`, null, {
            params: { reason }
        });
    },

    updateStatus: async (id: string, input: UpdateStatusDto) => {
        await api.put(`/api/app/membership/${id}/status`, input);
    },

    // Event Management
    getEvents: async (input: any) => {
        const response = await api.get<PagedResultDto<EventListDto>>('/api/app/event-admin', { params: input });
        return response.data;
    },

    getEvent: async (id: string) => {
        const response = await api.get(`/api/app/event-admin/${id}`);
        return response.data;
    },

    createEvent: async (input: CreateEventDto) => {
        const response = await api.post('/api/app/event-admin/event', input);
        return response.data;
    },

    updateEvent: async (id: string, input: UpdateEventDto) => {
        const response = await api.put(`/api/app/event-admin/${id}/event`, input);
        return response.data;
    },

    deleteEvent: async (id: string) => {
        await api.delete(`/api/app/event-admin/${id}/event`);
    },

    publishEvent: async (id: string) => {
        await api.post(`/api/app/event-admin/${id}/publish-event`);
    },

    // Content (CMS) & News
    getPosts: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<BlogPostDto>>('/api/app/blog-admin', { params: { ...input, keyword: input.filter } });
        return response.data;
    },
    getCategoryLookup: async () => {
        const response = await api.get<{ items: import('../types/news').ArticleCategoryLookupDto[] }>('/api/app/blog/category-lookup');
        return response.data;
    },
    createPost: async (input: CreateBlogPostDto) => {
        const response = await api.post<BlogPostDto>('/api/app/blog/post', input);
        return response.data;
    },
    updatePost: async (id: string, input: UpdateBlogPostDto) => {
        const response = await api.put<BlogPostDto>(`/api/app/blog/${id}/post`, input);
        return response.data;
    },
    deletePost: async (id: string) => {
        await api.delete(`/api/app/blog/${id}/post`);
    },
    getPost: async (id: string) => {
        const response = await api.get<BlogPostDto>(`/api/app/blog/${id}`);
        return response.data;
    },
    publishPost: async (id: string) => {
        await api.post(`/api/app/blog-admin/${id}/publish-post`);
    },
    unpublishPost: async (id: string) => {
        await api.post(`/api/app/blog-admin/${id}/unpublish-post`);
    },

    // Career
    getCareerServices: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<CareerServiceDto>>('/api/app/career/services', { params: input });
        return response.data;
    },
    createCareerService: async (input: CreateCareerServiceDto) => {
        const response = await api.post<CareerServiceDto>('/api/app/career/service', input);
        return response.data;
    },
    updateCareerService: async (params: { id: string, input: CreateCareerServiceDto }) => {
        const response = await api.put<CareerServiceDto>(`/api/app/career/${params.id}/service`, params.input);
        return response.data;
    },
    deleteCareerService: async (id: string) => {
        await api.delete(`/api/app/career/${id}/service`);
    },
    getCareerLookups: async () => {
        const response = await api.get<import('../types/career').CareerLookupsDto>('/api/app/career/lookups');
        return response.data;
    },

    // Benefits
    getGrants: async (input: PagedAndSortedResultRequestDto) => {
        const response = await api.get<PagedResultDto<AcademicGrantDto>>('/api/app/benefits/grants', { params: input });
        return response.data;
    },
    createGrant: async (input: CreateAcademicGrantDto) => {
        const response = await api.post<AcademicGrantDto>('/api/app/benefits/grant', input);
        return response.data;
    },
    updateGrant: async (id: string, input: CreateAcademicGrantDto) => {
        const response = await api.put<AcademicGrantDto>(`/api/app/benefits/${id}/grant`, input);
        return response.data;
    },
    deleteGrant: async (id: string) => {
        await api.delete(`/api/app/benefits/${id}/grant`);
    },
    getDiscounts: async (input: PagedAndSortedResultRequestDto) => {
        const response = await api.get<PagedResultDto<CommercialDiscountDto>>('/api/app/benefits/discounts', { params: input });
        return response.data;
    },
    createDiscount: async (input: CreateCommercialDiscountDto) => {
        const response = await api.post<CommercialDiscountDto>('/api/app/benefits/discount', input);
        return response.data;
    },
    updateDiscount: async (id: string, input: CreateCommercialDiscountDto) => {
        const response = await api.put<CommercialDiscountDto>(`/api/app/benefits/${id}/discount`, input);
        return response.data;
    },
    deleteDiscount: async (id: string) => {
        await api.delete(`/api/app/benefits/${id}/discount`);
    },

    // Gallery
    getAlbums: async (input: PagedAndSortedResultRequestDto) => {
        const response = await api.get<PagedResultDto<GalleryAlbumDto>>('/api/app/gallery/albums', { params: input });
        return response.data;
    },
    createAlbum: async (input: CreateGalleryAlbumDto) => {
        const response = await api.post<GalleryAlbumDto>('/api/app/gallery/album', {
            name: input.title,
            description: input.description
        });
        return response.data;
    },
    deleteAlbum: async (id: string) => {
        await api.delete(`/api/app/gallery/${id}/album`);
    },
    getAlbumDetails: async (id: string) => {
        const response = await api.get(`/api/app/gallery/${id}/album-details`);
        return response.data;
    },
    uploadAlbumImages: async (albumId: string, files: File[]) => {
        const imageBytes: string[] = [];
        const fileNames: string[] = [];

        for (const file of files) {
            const buffer = await file.arrayBuffer();
            const base64 = btoa(
                new Uint8Array(buffer).reduce((data, byte) => data + String.fromCharCode(byte), '')
            );
            imageBytes.push(base64);
            fileNames.push(file.name);
        }

        await api.post('/api/app/gallery/upload-images', {
            albumId,
            imageBytes,
            fileNames
        });
    },
    deleteMediaItem: async (albumId: string, mediaItemId: string) => {
        await api.delete('/api/app/gallery-admin/media-item', {
            params: { albumId, mediaItemId }
        });
    },

    // Syndicates
    getSyndicateRequests: async (input: any) => {
        const response = await api.get('/api/app/syndicate-admin/subscriptions', { params: input });
        return response.data;
    },
    getSyndicateDocument: async (subscriptionId: string, documentId: string) => {
        const response = await api.get(`/api/app/syndicate-admin/document/${subscriptionId}/${documentId}`, {
            responseType: 'blob'
        });
        return response.data;
    },
    updateSyndicateRequestStatus: async (id: string, status: SyndicateStatus) => {
        const response = await api.put(`/api/app/syndicate/${id}/request-status`, null, { params: { newStatus: status } });
        return response.data;
    },
    markSyndicateInProgress: async (id: string) => {
        await api.post(`/api/app/syndicate-admin/${id}/mark-as-in-progress`);
    },
    markSyndicateReadyForPickup: async (id: string) => {
        await api.post(`/api/app/syndicate-admin/${id}/mark-as-ready-for-pickup`);
    },
    markSyndicateReceived: async (id: string) => {
        await api.post(`/api/app/syndicate-admin/${id}/mark-as-received`);
    },
    rejectSyndicate: async (id: string, reason: string) => {
        await api.post(`/api/app/syndicate-admin/${id}/reject`, null, { params: { reason } });
    },

    // Health (Medical Partners)
    getMedicalPartners: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<MedicalPartnerDto>>('/api/app/medical-partner', { params: input });
        return response.data;
    },
    createMedicalPartner: async (input: CreateMedicalPartnerDto) => {
        const response = await api.post<MedicalPartnerDto>('/api/app/medical-partner', input);
        return response.data;
    },
    updateMedicalPartner: async (id: string, input: UpdateMedicalPartnerDto) => {
        const response = await api.put<MedicalPartnerDto>(`/api/app/medical-partner/${id}`, input);
        return response.data;
    },
    deleteMedicalPartner: async (id: string) => {
        await api.delete(`/api/app/medical-partner/${id}`);
    },
    getHealthcareOffers: async (input: any) => {
        const response = await api.get('/api/app/healthcare-offer-management', { params: input });
        return response.data;
    },

    // Certificates
    getCertificateRequests: async (input: any) => {
        const response = await api.get('/api/app/certificate-request', { params: input });
        return response.data;
    },
    updateCertificateStatus: async (id: string, input: UpdateCertificateStatusDto) => {
        const response = await api.put(`/api/app/certificate-request/${id}/status`, input);
        return response.data;
    },

    // Organization (Branches)
    getBranches: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<BranchDto>>('/api/app/branch', { params: input });
        return response.data;
    },
    createBranch: async (input: CreateUpdateBranchDto) => {
        const response = await api.post<BranchDto>('/api/app/branch', input);
        return response.data;
    },
    updateBranch: async (id: string, input: CreateUpdateBranchDto) => {
        const response = await api.put<BranchDto>(`/api/app/branch/${id}`, input);
        return response.data;
    },
    deleteBranch: async (id: string) => {
        await api.delete(`/api/app/branch/${id}`);
    },

    // Trips
    getTrips: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<AlumniTripDto>>('/api/app/trip/active-trips', { params: input });
        return response.data;
    },
    createTrip: async (input: CreateTripDto) => {
        const response = await api.post<AlumniTripDto>('/api/app/trip', input);
        return response.data;
    },
    deleteTrip: async (id: string) => {
        await api.delete(`/api/app/trip/${id}`);
    },

    // Guidance
    getAdvisingRequests: async (input: PagedAndSortedResultRequestDto & { status?: number }) => {
        const response = await api.get('/api/app/advising', { params: input });
        return response.data;
    },
    updateAdvisingStatus: async (id: string, input: any) => {
        const response = await api.put(`/api/app/advising/${id}/status`, input);
        return response.data;
    },

    // Communication
    sendMessage: async (input: any) => {
        await api.post('/api/app/communication/message', input);
    },
    getRecipientsCount: async (input: any) => {
        const response = await api.post('/api/app/communication/recipients-count', input);
        return response.data;
    },

    // Dashboard
    getDashboardStats: async () => {
        const response = await api.get<DashboardStatsDto>('/api/app/admin-dashboard/overview');
        return response.data;
    }
};
