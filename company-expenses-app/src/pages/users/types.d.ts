export enum InvitationStatus {
  Pending = 0,
  Accepted = 1,
  Declined = 2,
  Expired = 3,
  Cancelled = 4,
}

export interface Workplace {
  id: string;
  name: string;
  description?: string;
  address?: string;
  isActive: boolean;
}

export interface Invitation {
  id: string;
  email: string;
  invitedRoleId?: string;
  workplaceId?: string;
  token: string;
  expiresAt: string;
  acceptedAt?: string;
  invitedByUserId: string;
  status: InvitationStatus;
  createdAt: string;
  createdBy: string;
  workplace?: Workplace;
}
