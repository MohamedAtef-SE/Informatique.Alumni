import { lazy } from 'react';
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import AuthLayout from './components/layouts/AuthLayout';
import MainLayout from './components/layouts/MainLayout';

import Callback from './pages/auth/Callback';

// Placeholder Pages
import Login from './pages/auth/Login';
import Register from './pages/auth/Register';
import PortalHome from './pages/portal/PortalHome';
import Directory from './pages/portal/Directory';
import AlumniProfile from './pages/portal/AlumniProfile';
import CareerServicesList from './pages/portal/CareerServicesList';
import CareerServiceDetail from './pages/portal/CareerServiceDetail';
import EventsList from './pages/portal/EventsList';
import EventDetail from './pages/portal/EventDetail';
import ServicesLayout from './pages/portal/ServicesLayout';
import Gallery from './pages/portal/Gallery';
import Profile from './pages/portal/Profile';
import Calendar from './pages/portal/Calendar';
import Magazine from './pages/portal/Magazine';
import TripsList from './pages/portal/TripsList';
import MyWallet from './pages/portal/MyWallet';
const MyRegistrations = lazy(() => import('./pages/portal/MyRegistrations'));
import AdminLayout from './components/layouts/AdminLayout';
import AdminGuard from './components/auth/AdminGuard';
import AlumniGuard from './components/auth/AlumniGuard';
import AdminDashboard from './pages/admin/AdminDashboard';
import AlumniManager from './pages/admin/AlumniManager';
import EventManager from './pages/admin/EventManager';
import ParticipantManager from './pages/admin/ParticipantManager';
import ContentManager from './pages/admin/ContentManager';
import CareerManager from './pages/admin/CareerManager';
import CareerServiceTypeManager from './pages/admin/CareerServiceTypeManager';
import JobManager from './pages/admin/JobManager';
import CompanyManager from './pages/admin/CompanyManager';
import BenefitsManager from './pages/admin/BenefitsManager';
import GalleryManager from './pages/admin/GalleryManager';
import MagazineManager from './pages/admin/MagazineManager';
import SyndicatesManager from './pages/admin/SyndicatesManager';
import HealthManager from './pages/admin/HealthManager';
import CertificatesManager from './pages/admin/CertificatesManager';
import OrgManager from './pages/admin/OrgManager';
import CollegeManager from './pages/admin/CollegeManager';
import MajorManager from './pages/admin/MajorManager';
import TripsManager from './pages/admin/TripsManager';
import GuidanceManager from './pages/admin/GuidanceManager';
import AdvisoryCategoryManager from './pages/admin/AdvisoryCategoryManager';
import CommunicationCenter from './pages/admin/CommunicationCenter';
import SessionExpiredModal from './components/common/SessionExpiredModal';
const NotFound = () => <div className="p-8"><h1>404 Not Found</h1></div>;

function App() {
  return (
    <BrowserRouter>
      <SessionExpiredModal />
      <Routes>
        {/* Auth Routes */}
        <Route path="/auth" element={<AuthLayout />}>
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="callback" element={<Callback />} />
          <Route index element={<Navigate to="/auth/login" replace />} />
        </Route>

        {/* Protected Portal Routes */}
        <Route path="/portal" element={<AlumniGuard />}>
          <Route element={<MainLayout />}>
            <Route index element={<PortalHome />} />
            <Route path="directory" element={<Directory />} />
            <Route path="directory/:id" element={<AlumniProfile />} />
            <Route path="career" element={<CareerServicesList />} />
            <Route path="career/:id" element={<CareerServiceDetail />} />
            <Route path="events" element={<EventsList />} />
            <Route path="events/:id" element={<EventDetail />} />
            <Route path="services" element={<ServicesLayout />} />
            <Route path="gallery" element={<Gallery />} />
            <Route path="profile" element={<Profile />} />
            <Route path="calendar" element={<Calendar />} />
            <Route path="magazine" element={<Magazine />} />
            <Route path="trips" element={<TripsList />} />
            <Route path="wallet" element={<MyWallet />} />
            <Route path="registrations" element={<MyRegistrations />} />
          </Route>
        </Route>

        {/* Admin Routes */}
        <Route path="/admin" element={<AdminGuard />}>
          <Route element={<AdminLayout />}>
            <Route index element={<Navigate to="/admin/dashboard" replace />} />
            <Route path="dashboard" element={<AdminDashboard />} />
            <Route path="alumni" element={<AlumniManager />} />
            {/* Placeholders for now */}
            <Route path="events" element={<EventManager />} />
            <Route path="event-registrations" element={<ParticipantManager />} />
            <Route path="content" element={<ContentManager />} />
            <Route path="career" element={<CareerManager />} />
            <Route path="career-types" element={<CareerServiceTypeManager />} />
            <Route path="jobs" element={<JobManager />} />
            <Route path="companies" element={<CompanyManager />} />
            <Route path="benefits" element={<BenefitsManager />} />
            <Route path="gallery" element={<GalleryManager />} />
            <Route path="magazine" element={<MagazineManager />} />
            <Route path="syndicates" element={<SyndicatesManager />} />
            <Route path="health" element={<HealthManager />} />
            <Route path="certificates" element={<CertificatesManager />} />
            <Route path="organization">
              <Route index element={<OrgManager />} />
              <Route path="colleges" element={<CollegeManager />} />
              <Route path="majors" element={<MajorManager />} />
            </Route>
            <Route path="trips" element={<TripsManager />} />
            <Route path="guidance" element={<GuidanceManager />} />
            <Route path="advisory-categories" element={<AdvisoryCategoryManager />} />
            <Route path="communication" element={<CommunicationCenter />} />
          </Route>
        </Route>

        {/* Redirects */}
        <Route path="/" element={<Navigate to="/portal" replace />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
