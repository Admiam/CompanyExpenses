# Project Structure - Constants, Types & Utils

This document describes the global constants, types, and utility functions used across the application.

## Constants

Located in `/src/constants/`

### Invitation Constants (`/constants/invitation.ts`)

- `InvitationStatus` - Enum-like constant object for invitation statuses
- `InvitationStatusType` - TypeScript type for invitation status values

### Role Constants (`/constants/roles.ts`)

- `roleLabels` - Human-readable labels for user roles
- `roleColors` - Tailwind CSS classes for role badges
- `RoleType` - TypeScript type for role values

### Usage Example

```typescript
import { InvitationStatus, roleLabels, roleColors } from "@/constants";

// Use invitation status
if (invitation.status === InvitationStatus.Pending) {
  // Handle pending invitation
}

// Use role labels
const label = roleLabels.admin; // "Administrátor"
const colorClass = roleColors.admin; // "bg-purple-500/10 text-purple-500"
```

## Types

Located in `/src/lib/proxy/types.d.ts`

### Invitation Types

- `Invitation` - Complete invitation object
- `InvitationStatusType` - Type for invitation status values
- `CreateInvitationRequest` - Request payload for creating invitations
- `AcceptInvitationRequest` - Request payload for accepting invitations

### Workplace Types

- `Workplace` - Workplace entity
- `WorkplaceMember` - Workplace member entity
- `CreateWorkplaceRequest` / `UpdateWorkplaceRequest` - Request payloads

### Usage Example

```typescript
import type { Invitation } from "@/lib/proxy/types";

const invitation: Invitation = {
  id: "...",
  email: "user@example.com",
  status: InvitationStatus.Pending,
  // ... other fields
};
```

## Utility Functions

Located in `/src/utils/`

### Invitation Utils (`/utils/invitation.tsx`)

- `getInvitationStatusLabel(status)` - Returns human-readable label for status
- `getInvitationStatusIcon(status)` - Returns React icon component for status

### Usage Example

```typescript
import { getInvitationStatusLabel, getInvitationStatusIcon } from "@/utils";

// In your component
<div>
  {getInvitationStatusIcon(invitation.status)}
  <span>{getInvitationStatusLabel(invitation.status)}</span>
</div>;
```

## Why This Structure?

1. **Reusability** - Constants and utilities can be used across the entire application
2. **Maintainability** - Single source of truth for status values, labels, and colors
3. **Type Safety** - TypeScript types ensure correct usage throughout the app
4. **Consistency** - All components use the same labels and styling
5. **Scalability** - Easy to add new constants and utilities as the app grows

## Adding New Constants/Utils

1. Create the file in the appropriate directory (`/constants/`, `/utils/`)
2. Export from the index file (`/constants/index.ts`, `/utils/index.ts`)
3. Import using the alias: `import { ... } from '@/constants'` or `import { ... } from '@/utils'`
