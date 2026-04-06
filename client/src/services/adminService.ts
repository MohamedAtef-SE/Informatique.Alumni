import { api } from './api';
import type { PagedResultDto, PagedAndSortedResultRequestDto } from '../types/common';
import type { AssociationRequestDto, MembershipRequestFilterDto, UpdateStatusDto } from '../types/membership';
import type { EventListDto, CreateEventDto, UpdateEventDto } from '../types/events';
import type { BlogPostDto, CreateBlogPostDto, UpdateBlogPostDto } from '../types/news';
import type { CareerServiceDto, CreateCareerServiceDto, CareerServiceTypeDto, CreateUpdateCareerServiceTypeDto, CareerServiceTypeFilterDto, JobDto, JobAdminDto, JobApplicationAdminDto, JobAdminGetListInput } from '../types/career';
import type { AcademicGrantDto, CommercialDiscountDto, CreateAcademicGrantDto, CreateCommercialDiscountDto } from '../types/benefits';
import type { GalleryAlbumDto } from '../types/gallery';
import type { UpdateCertificateStatusDto, CertificateDefinitionDto, CreateCertificateDefinitionDto, UpdateCertificateDefinitionDto } from '../types/certificates';
import type { SyndicateStatus } from '../types/syndicates';
import type { MedicalPartnerDto, CreateMedicalPartnerDto, UpdateMedicalPartnerDto, MedicalCategoryDto, CreateUpdateMedicalCategoryDto } from '../types/health';
import type { BranchDto, CreateUpdateBranchDto, CollegeDto, CreateUpdateCollegeDto, MajorDto, CreateUpdateMajorDto } from '../types/organization';
import type { TripAdminDto, TripRequestAdminDto, CreateTripInput } from '../types/trips';
import type { DashboardStatsDto, AlumniAdminListDto, AlumniAdminDto, AlumniAdminGetListInput, AlumniCvDto } from '../types/admin';
import type { CurrencyDto, LookupItemDto } from '../types/lookups';
import type { CompanyDto, CreateUpdateCompanyDto, CompanyFilterDto } from '../types/company';

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

    getAlumniProfiles: async (input: AlumniAdminGetListInput) => {
        const response = await api.get<PagedResultDto<AlumniAdminListDto>>('/api/app/alumni-admin/profiles', {
            params: input
        });
        return response.data;
    },

    toggleAlumniVip: async (id: string) => {
        await api.post(`/api/app/alumni-admin/${id}/mark-as-notable`);
    },

    getAlumniProfile: async (id: string) => {
        const response = await api.get<AlumniAdminDto>(`/api/app/alumni-admin/${id}`);
        return response.data;
    },

    toggleAlumniAdvisor: async (id: string) => {
        await api.post(`/api/app/alumni-admin/${id}/toggle-advisor`);
    },

    approveAlumniAdvisor: async (id: string) => {
        await api.post(`/api/app/alumni-admin/${id}/approve-advisor`);
    },

    rejectAlumniAdvisor: async (id: string, reason: string) => {
        await api.post(`/api/app/alumni-admin/${id}/reject-advisor`, { reason });
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
    getEventParticipants: async (input: import('../types/events').ActivityParticipantFilterDto) => {
        const response = await api.get<PagedResultDto<import('../types/events').ActivityParticipantDto>>('/api/app/events/participants', { params: input });
        return response.data;
    },
    approveEventRegistration: async (id: string) => {
        await api.post(`/api/app/events/${id}/approve-registration`);
    },
    rejectEventRegistration: async (id: string, reason: string) => {
        await api.post(`/api/app/events/${id}/reject-registration`, null, { params: { reason } });
    },
    getCompanies: async (input: CompanyFilterDto) => {
        const response = await api.get<PagedResultDto<CompanyDto>>('/api/app/company', { params: input });
        return response.data;
    },

    getCompany: async (id: string) => {
        const response = await api.get<CompanyDto>(`/api/app/company/${id}`);
        return response.data;
    },

    createCompany: async (input: CreateUpdateCompanyDto) => {
        const formData = new FormData();
        formData.append('nameAr', input.nameAr);
        formData.append('nameEn', input.nameEn);
        if (input.websiteUrl) formData.append('websiteUrl', input.websiteUrl);
        if (input.industry) formData.append('industry', input.industry);
        if (input.description) formData.append('description', input.description);
        if (input.email) formData.append('email', input.email);
        if (input.phoneNumber) formData.append('phoneNumber', input.phoneNumber);
        if (input.logo) formData.append('logo', input.logo);

        const response = await api.post<string>('/api/app/company', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
        return response.data;
    },

    updateCompany: async (id: string, input: CreateUpdateCompanyDto) => {
        const formData = new FormData();
        formData.append('nameAr', input.nameAr);
        formData.append('nameEn', input.nameEn);
        if (input.websiteUrl) formData.append('websiteUrl', input.websiteUrl);
        if (input.industry) formData.append('industry', input.industry);
        if (input.description) formData.append('description', input.description);
        if (input.email) formData.append('email', input.email);
        if (input.phoneNumber) formData.append('phoneNumber', input.phoneNumber);
        formData.append('isActive', String(input.isActive ?? true));
        if (input.logo) formData.append('logo', input.logo);

        await api.put(`/api/app/company/${id}`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
        });
    },

    deleteCompany: async (id: string) => {
        await api.delete(`/api/app/company/${id}`);
    },

    getCompanyLookup: async () => {
        const response = await api.get<CompanyDto[]>('/api/app/events/companies');
        return response.data;
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
        await api.post(`/api/app/blog-admin/${id}/publish`);
    },
    unpublishPost: async (id: string) => {
        await api.post(`/api/app/blog-admin/${id}/unpublish`);
    },

    // CMS Categories
    getContentCategories: async (input: PagedAndSortedResultRequestDto) => {
        const response = await api.get<PagedResultDto<import('../types/news').ArticleCategoryDto>>('/api/app/blog-admin/categories', { params: input });
        return response.data;
    },
    createContentCategory: async (input: import('../types/news').CreateUpdateArticleCategoryDto) => {
        const response = await api.post<import('../types/news').ArticleCategoryDto>('/api/app/blog-admin/category', input);
        return response.data;
    },
    updateContentCategory: async (id: string, input: import('../types/news').CreateUpdateArticleCategoryDto) => {
        const response = await api.put<import('../types/news').ArticleCategoryDto>(`/api/app/blog-admin/${id}/category`, input);
        return response.data;
    },
    deleteContentCategory: async (id: string) => {
        await api.delete(`/api/app/blog-admin/${id}/category`);
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

    // Career Service Types
    getCareerServiceTypes: async (input: CareerServiceTypeFilterDto) => {
        const response = await api.get<PagedResultDto<CareerServiceTypeDto>>('/api/app/career-service-type', {
            params: input
        });
        return response.data;
    },
    getCareerServiceType: async (id: string) => {
        const response = await api.get<CareerServiceTypeDto>(`/api/app/career-service-type/${id}`);
        return response.data;
    },
    createCareerServiceType: async (input: CreateUpdateCareerServiceTypeDto) => {
        const response = await api.post<CareerServiceTypeDto>('/api/app/career-service-type', input);
        return response.data;
    },
    updateCareerServiceType: async (id: string, input: CreateUpdateCareerServiceTypeDto) => {
        const response = await api.put<CareerServiceTypeDto>(`/api/app/career-service-type/${id}`, input);
        return response.data;
    },
    deleteCareerServiceType: async (id: string) => {
        await api.delete(`/api/app/career-service-type/${id}`);
    },

    // Job Management
    getAdminJobs: async (input: JobAdminGetListInput) => {
        const response = await api.get<PagedResultDto<JobAdminDto>>('/api/app/job-admin', { params: input });
        return response.data;
    },
    getAdminJob: async (id: string) => {
        const response = await api.get<JobAdminDto>(`/api/app/job-admin/${id}`);
        return response.data;
    },
    createJob: async (input: Partial<JobDto>) => {
        const response = await api.post<JobDto>('/api/app/job/job', input);
        return response.data;
    },
    approveJob: async (id: string) => {
        await api.post(`/api/app/job-admin/${id}/approve-job`);
    },
    rejectJob: async (id: string) => {
        await api.post(`/api/app/job-admin/${id}/reject-job`);
    },
    getJobApplications: async (jobId: string, input: PagedAndSortedResultRequestDto) => {
        const response = await api.get<PagedResultDto<JobApplicationAdminDto>>(`/api/app/job-admin/applications/${jobId}`, { params: input });
        return response.data;
    },
    getApplicationCv: async (id: string) => {
        const response = await api.get(`/api/app/job-admin/${id}/application-cv`, {
            responseType: 'blob'
        });
        return response.data;
    },
    getAlumniCv: async (alumniId: string) => {
        const response = await api.get<AlumniCvDto>(`/api/app/job-admin/alumni-cv/${alumniId}`);
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
    createAlbum: async (input: { name: string, description?: string }) => {
        const response = await api.post<GalleryAlbumDto>('/api/app/gallery/album', {
            name: input.name,
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
    getMedicalCategories: async (input: PagedAndSortedResultRequestDto & { filterText?: string, isActive?: boolean }) => {
        const response = await api.get<PagedResultDto<MedicalCategoryDto>>('/api/app/medical-category', { params: input });
        return response.data;
    },
    createMedicalCategory: async (input: CreateUpdateMedicalCategoryDto) => {
        const response = await api.post<MedicalCategoryDto>('/api/app/medical-category', input);
        return response.data;
    },
    updateMedicalCategory: async (id: string, input: CreateUpdateMedicalCategoryDto) => {
        const response = await api.put<MedicalCategoryDto>(`/api/app/medical-category/${id}`, input);
        return response.data;
    },
    deleteMedicalCategory: async (id: string) => {
        await api.delete(`/api/app/medical-category/${id}`);
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
    getCertificateDefinitions: async (input: PagedAndSortedResultRequestDto) => {
        const response = await api.get<PagedResultDto<CertificateDefinitionDto>>('/api/app/certificate-definition', { params: input });
        return response.data;
    },
    getCertificateDefinition: async (id: string) => {
        const response = await api.get<CertificateDefinitionDto>(`/api/app/certificate-definition/${id}`);
        return response.data;
    },
    createCertificateDefinition: async (input: CreateCertificateDefinitionDto) => {
        const response = await api.post<CertificateDefinitionDto>('/api/app/certificate-definition', input);
        return response.data;
    },
    updateCertificateDefinition: async (id: string, input: UpdateCertificateDefinitionDto) => {
        const response = await api.put<CertificateDefinitionDto>(`/api/app/certificate-definition/${id}`, input);
        return response.data;
    },
    deleteCertificateDefinition: async (id: string) => {
        await api.delete(`/api/app/certificate-definition/${id}`);
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

    // Organization (Colleges)
    getColleges: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<CollegeDto>>('/api/app/college', { params: input });
        return response.data;
    },
    createCollege: async (input: CreateUpdateCollegeDto) => {
        const response = await api.post<CollegeDto>('/api/app/college', input);
        return response.data;
    },
    updateCollege: async (id: string, input: CreateUpdateCollegeDto) => {
        const response = await api.put<CollegeDto>(`/api/app/college/${id}`, input);
        return response.data;
    },
    deleteCollege: async (id: string) => {
        await api.delete(`/api/app/college/${id}`);
    },

    // Organization (Majors)
    getMajors: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<MajorDto>>('/api/app/major', { params: input });
        return response.data;
    },
    createMajor: async (input: CreateUpdateMajorDto) => {
        const response = await api.post<MajorDto>('/api/app/major', input);
        return response.data;
    },
    updateMajor: async (id: string, input: CreateUpdateMajorDto) => {
        const response = await api.put<MajorDto>(`/api/app/major/${id}`, input);
        return response.data;
    },
    deleteMajor: async (id: string) => {
        await api.delete(`/api/app/major/${id}`);
    },

    // Trips — Admin (uses TripAdminAppService → /api/app/trip-admin/*)
    getTrips: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<TripAdminDto>>('/api/app/trip-admin/trips', { params: input });
        return response.data;
    },
    createTrip: async (input: CreateTripInput) => {
        const response = await api.post<TripAdminDto>('/api/app/trip/trip', input);
        return response.data;
    },
    deleteTrip: async (id: string) => {
        await api.delete(`/api/app/trip-admin/${id}/trip`);
    },
    activateTrip: async (id: string) => {
        await api.post(`/api/app/trip-admin/${id}/activate-trip`);
    },
    deactivateTrip: async (id: string) => {
        await api.post(`/api/app/trip-admin/${id}/deactivate-trip`);
    },
    getTripRequests: async (tripId: string, input: PagedAndSortedResultRequestDto) => {
        const response = await api.get<PagedResultDto<TripRequestAdminDto>>(`/api/app/trip-admin/requests/${tripId}`, { params: input });
        return response.data;
    },
    approveTripRequest: async (requestId: string) => {
        await api.post(`/api/app/trip-admin/approve-request/${requestId}`);
    },
    rejectTripRequest: async (requestId: string) => {
        await api.post(`/api/app/trip-admin/reject-request/${requestId}`);
    },

    // Lookups (AllowAnonymous — no auth required)
    getCurrencies: async (): Promise<CurrencyDto[]> => {
        const response = await api.get<CurrencyDto[]>('/api/app/lookup/currencies');
        return response.data;
    },
    getTripTypes: async (): Promise<LookupItemDto[]> => {
        const response = await api.get<LookupItemDto[]>('/api/app/lookup/trip-types');
        return response.data;
    },

    // Guidance (Admin)
    getAdvisingRequests: async (input: PagedAndSortedResultRequestDto & { status?: number }) => {
        const response = await api.get('/api/app/guidance-admin', { params: input });
        return response.data;
    },
    approveAdvisingRequest: async (id: string, meetingLink?: string) => {
        const response = await api.post(`/api/app/guidance-admin/${id}/approve-request`, { meetingLink });
        return response.data;
    },
    rejectAdvisingRequest: async (id: string, _notes?: string) => {
        // Backend currently ignores reason and sets "Rejected by admin" natively
        const response = await api.post(`/api/app/guidance-admin/${id}/reject-request`);
        return response.data;
    },
    getGuidanceRule: async (branchId: string) => {
        const response = await api.get(`/api/app/guidance-admin/rule`, { params: { branchId } });
        return response.data;
    },
    updateGuidanceRule: async (input: any) => {
        await api.post(`/api/app/guidance-admin/rule`, input);
    },

    // Communication
    sendMessage: async (input: any) => {
        await api.post('/api/app/communications/send-message', input);
    },
    getRecipientsCount: async (input: any) => {
        const response = await api.post('/api/app/communications/get-recipients-count', input);
        return response.data;
    },
    getDistinctGraduationYears: async (): Promise<number[]> => {
        const response = await api.get('/api/app/communications/distinct-graduation-years');
        return response.data;
    },
    getCommunicationLogs: async (input: {
        filterText?: string;
        channel?: string;
        status?: string;
        recipientId?: string;
        skipCount?: number;
        maxResultCount?: number;
        sorting?: string;
    }) => {
        const response = await api.post('/api/app/communications/search-logs', input);
        return response.data;
    },

    // Magazine Management
    getMagazines: async (input: PagedAndSortedResultRequestDto & { filter?: string }) => {
        const response = await api.get<PagedResultDto<import('../types/news').MagazineIssueDto>>('/api/app/magazine/issues', { params: input });
        return response.data;
    },

    createMagazine: async (input: { title: string; description?: string; publishDate: string; file: File }) => {
        const buffer = await input.file.arrayBuffer();
        const uint8Array = new Uint8Array(buffer);
        let binary = '';
        const len = uint8Array.byteLength;
        for (let i = 0; i < len; i++) {
            binary += String.fromCharCode(uint8Array[i]);
        }
        const base64 = window.btoa(binary);
        
        const response = await api.post<import('../types/news').MagazineIssueDto>('/api/app/magazine/issue', {
            title: input.title,
            description: input.description,
            publishDate: input.publishDate,
            pdfBytes: base64,
            pdfFileName: input.file.name
        });
        return response.data;
    },

    deleteMagazine: async (id: string) => {
        await api.delete(`/api/app/magazine/${id}/issue`);
    },

    // Dashboard
    getDashboardStats: async () => {
        const response = await api.get<DashboardStatsDto>('/api/app/admin-dashboard/overview');
        return response.data;
    },

    // Alumni Import
    importAlumniExcel: async (file: File) => {
        const formData = new FormData();
        // ABP binds IRemoteStreamContent by the C# parameter name ('stream'), not 'file'
        formData.append('stream', file);
        const response = await api.post('/api/app/alumni-import/import-excel', formData, {
            headers: {
                'Content-Type': 'multipart/form-data'
            }
        });
        return response.data;
    },
    downloadImportTemplate: async (): Promise<Blob> => {
        const response = await api.get('/api/app/alumni-import/import-template', {
            responseType: 'blob'
        });
        return response.data;
    }
};
