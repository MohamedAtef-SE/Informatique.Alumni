

export const CommunicationChannel = {
    Email: 1,
    Sms: 2
} as const;
export type CommunicationChannel = typeof CommunicationChannel[keyof typeof CommunicationChannel];

export const CommunicationMembershipStatus = {
    All: 0,
    Active: 1,
    Inactive: 2
} as const;
export type CommunicationMembershipStatus = typeof CommunicationMembershipStatus[keyof typeof CommunicationMembershipStatus];

export interface AlumniCommunicationFilterDto {
    branchId: string;
    graduationYear?: number;
    graduationSemester?: number;
    collegeId?: string;
    majorId?: string;
    minorId?: string;
    membershipStatus?: CommunicationMembershipStatus;
    gpaFrom?: number;
    gpaTo?: number;
}

export interface SendGeneralMessageInputDto {
    channel: CommunicationChannel;
    subject: string;
    body: string;
    attachmentUrls?: string[];
    filter: AlumniCommunicationFilterDto;
}
