export interface PagedResultDto<T> {
    totalCount: number;
    items: T[];
}

export interface PagedAndSortedResultRequestDto {
    skipCount?: number;
    maxResultCount?: number;
    sorting?: string;
    filter?: string;
}

export interface ListResultDto<T> {
    items: T[];
}

export interface EntityDto<TKey> {
    id: TKey;
}

export interface FullAuditedEntityDto<TKey> extends EntityDto<TKey> {
    creationTime?: string;
    creatorId?: string;
    lastModificationTime?: string;
    lastModifierId?: string;
    isDeleted?: boolean;
    deleterId?: string;
    deletionTime?: string;
}
