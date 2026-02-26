**Role:**
Act as a Senior .NET Backend Architect expert in ABP Framework (Commercial/Open Source), Domain-Driven Design (DDD), and Enterprise API Security.

**Project Context:**
I have completed the **Public Alumni Portal** (Domain & Application Layers). It includes multiple modules (Profiles, Jobs, Events, etc.) with custom business logic.
* **Current Status:** The Public APIs are done. The **Admin APIs** are outdated, incomplete, or missing entirely.
* **Goal:** Build a robust, "Best Practice" Admin Backend API layer.
* **Reference Standard:** Use **American University in Cairo (AUC) Alumni Portal** as the functional benchmark (e.g., strong verification, ID card management, mentorship vetting, notable alumni highlighting).

**Task 1: The "Deep Scan" & Gap Analysis**
Iterate through my `Domain` project. For **EACH** Aggregate Root you find (e.g., `Alumni`, `JobPost`, `Event`, `Story`, `Mentorship`), compare it against the existing `Admin.Application` project.
* **Identify:** Which entities have *no* Admin AppService?
* **Analyze:** For entities that *do* have an Admin Service, are they missing new properties I added (e.g., `GraduationYear`, `LinkedInProfile`, `isLookingForJob`)?

**Task 2: Refactor Core Alumni Admin API (The "AUC Standard")**
Rewrite the `AlumniAdminAppService` to support robust lifecycle management. It must include:
1.  **Full Profile Access:** Ensure the `Get` DTO returns *all* data (including sensitive fields hidden from public view).
2.  **Verification Workflow:**
    * `ApproveAlumni(Guid id)`: Sets status to Active + Assigns Role.
    * `RejectAlumni(Guid id, string reason)`: Captures rejection reason.
    * **AUC Feature:** Add a specific endpoint `UpdateAlumniIdCardStatus(Guid id, IdCardStatus status)` (e.g., Requested, Printed, Delivered) – common in university systems.
3.  **Directory Management:**
    * `MarkAsNotable(Guid id)`: To highlight specific alumni on the homepage (AUC "Notable Alumni" feature).
    * `BanAlumni(Guid id)`: Revokes access without deleting data.

**Task 3: Generate Missing Admin Modules**
For every other module found in my Domain, generate a standard `AdminAppService` with "Moderation" logic:
* **Jobs/Careers:** `ApproveJobPost`, `RejectJobPost`. (Admin must vet user-submitted roles).
* **Events:** `GetEventAttendees(Guid eventId)` (Admin needs to see who is coming).
* **Stories/Class Notes:** `PublishStory`, `UnpublishStory`.
* **Mentorship:** `ApproveMentorApplication` (if a mentorship aggregate exists).

**Task 4: Dashboard Analytics API**
Create a `DashboardAppService` that returns key administrative metrics:
* Count of Pending vs. Active Alumni.
* Alumni distribution by `GraduationYear` or `Major`.
* Total Donations (if Donation entity exists).

**Technical Constraints:**
* **Framework:** .NET 8, ABP Framework.
* **Security:** Apply `[Authorize(AlumniPermissions.Admin.Default)]` strictly.
* **Pattern:** Use `AsyncCrudAppService` where possible but override `Create/Update` to enforce Admin-only validation rules.

**Deliverables:**
1.  **Audit Report:** A bulleted list of the Modules/Aggregates you found in my code and what Admin APIs were missing.
2.  **C# Code:** The complete, refactored `AlumniAdminAppService`.
3.  **C# Code:** The new Admin AppServices for the other modules (Jobs, Events, etc.).
4.  **C# Code:** The `DashboardAppService`.