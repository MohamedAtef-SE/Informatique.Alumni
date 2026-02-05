# Implementation Plan - Dashboard Real Data

## Goal
Replace hardcoded mock data in `Dashboard.tsx` with real API data fetched via existing services (`careerService`, `eventsService`, `alumniService`).

## Proposed Changes

### [Client] Frontend

#### [MODIFY] [Dashboard.tsx](file:///d:/Projects/Informatique.Alumni/client/src/pages/portal/Dashboard.tsx)
- Import `useQuery` from `@tanstack/react-query`.
- Import `careerService`, `eventsService`, `alumniService`.
- Implement queries:
    - `alumniQuery`: Fetch total alumni count.
    - `jobsQuery`: Fetch total active jobs.
    - `eventsQuery`: Fetch upcoming events (count and list).
- Update **Stats Cards**:
    - **New Job Matches**: Show `jobsQuery.data?.totalCount` (default 0).
    - **Upcoming Events**: Show `eventsQuery.data?.totalCount` (default 0).
    - **Community Size** (Replace "Network Requests"): Show `alumniQuery.data?.totalCount`.
    - **Profile Views**: Keep static (API unavailable).
- Update **Recent Activity**:
    - Map `eventsQuery.data?.items` to show the latest events.
    - Add fallback "No recent activity" message.

## Verification Plan

### Manual Verification
1. **Login** as `devuser`.
2. **View Dashboard**:
   - Verify "Alumni Directory" count matches the Directory page.
   - Verify "Upcoming Events" count matches Events page.
   - Verify "Recent Activity" shows items from the events list.
3. **Empty State Check**:
   - If database is empty, verify stats show "0" and no errors occur.
