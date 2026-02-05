import type { EntityDto } from './common';

export interface BranchDto extends EntityDto<string> {
    name: string;
    code: string;
    address?: string;
}

export interface CreateUpdateBranchDto {
    name: string;
    code: string;
    address?: string;
}
