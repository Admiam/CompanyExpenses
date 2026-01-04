import { Mail, CheckCircle, XCircle } from "lucide-react";
import { InvitationStatus, type InvitationStatusType } from "@/constants/invitation";

export const getInvitationStatusLabel = (status: InvitationStatusType): string => {
  switch (status) {
    case InvitationStatus.Pending:
      return "Pending";
    case InvitationStatus.Accepted:
      return "Accepted";
    case InvitationStatus.Declined:
      return "Declined";
    case InvitationStatus.Expired:
      return "Expired";
    case InvitationStatus.Cancelled:
      return "Cancelled";
    default:
      return "Unknown";
  }
};

export const getInvitationStatusIcon = (status: InvitationStatusType) => {
  switch (status) {
    case InvitationStatus.Pending:
      return <Mail className="h-4 w-4 text-yellow-500" />;
    case InvitationStatus.Accepted:
      return <CheckCircle className="h-4 w-4 text-green-500" />;
    case InvitationStatus.Declined:
    case InvitationStatus.Cancelled:
      return <XCircle className="h-4 w-4 text-red-500" />;
    case InvitationStatus.Expired:
      return <XCircle className="h-4 w-4 text-gray-500" />;
    default:
      return null;
  }
};
