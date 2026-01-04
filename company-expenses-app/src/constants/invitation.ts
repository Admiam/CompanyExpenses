export const InvitationStatus = {
  Pending: 0,
  Accepted: 1,
  Declined: 2,
  Expired: 3,
  Cancelled: 4,
} as const;

export type InvitationStatusType = (typeof InvitationStatus)[keyof typeof InvitationStatus];
