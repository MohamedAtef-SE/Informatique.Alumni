# Enhancing Syndicate Admin Review Process

## Goal Description
Enhance the Syndicate Admin review process so the admin can identify the alumni (Name, National ID, Mobile) and review their uploaded documents before approving or rejecting the request. We will update the backend DTO to include these details, add an admin endpoint for document retrieval, and build a "Review Application" drawer in the frontend.

## Proposed Changes

### Backend Updates
*   **[MODIFY] `Phase2AdminContracts.cs`**:
    *   Add `AlumniName`, `AlumniNationalId`, `AlumniMobile` to `SyndicateSubscriptionAdminDto`.
    *   Add `List<SyndicateDocumentDto> Documents` to `SyndicateSubscriptionAdminDto`.
    *   Add `Task<IRemoteStreamContent> GetDocumentAsync(Guid subscriptionId, Guid documentId)` to `ISyndicateAdminAppService`.
*   **[MODIFY] `SyndicateAdminAppService.cs`**:
    *   Inject `IRepository<AlumniProfile, Guid>`, `IRepository<IdentityUser, Guid>`, and `IBlobContainer<SyndicateBlobContainer>`.
    *   Update `GetSubscriptionsAsync` to fetch `Documents` and join with `IdentityUser` and `AlumniProfile` to populate `AlumniName`, `AlumniNationalId`, and `AlumniMobile`.
    *   Implement `GetDocumentAsync(Guid subscriptionId, Guid documentId)` returning `RemoteStreamContent` from blob storage so admins can download files.

### Frontend Updates
*   **[MODIFY] `adminService.ts`**:
    *   Add `getSyndicateDocument: (subscriptionId: string, documentId: string)` mapped to the new admin endpoint `/api/app/syndicate-admin/document`.
*   **[MODIFY] `syndicates.ts` (types)**:
    *   Update `SyndicateSubscriptionAdmin` interface to include the new fields (`alumniName`, `alumniNationalId`, `alumniMobile`, `documents`).
*   **[MODIFY] `SyndicatesManager.tsx`**:
    *   Update the "Alumni" column in the table to show `alumniName` instead of `alumniId`.
    *   Add a "Review" action button that opens a new modal/drawer.
    *   The review drawer will display the full Alumni information, Subscription details, a list of uploaded documents with "View" buttons, and the action buttons (Approve, Reject, etc.).

## Verification Plan
1.  Admin logs in and goes to the Syndicate Applications tab.
2.  Admin sees real user names in the "Alumni" column.
3.  Admin clicks "Review" on an application to open the details view.
4.  Admin clicks "View" next to an uploaded document to verify the file opens correctly from the admin endpoints.
