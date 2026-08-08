# Spec 100: Tasks — Frontend Application (React + PWA)

## Task Checklist

### T10.1: Scaffold Vite project
- [ ] `npm create vite@latest frontend -- --template react-ts`
- [ ] Install dependencies: react-router-dom, tailwindcss, @tailwindcss/vite, axios, react-hot-toast, lucide-react, vite-plugin-pwa
- [ ] Configure Tailwind CSS 4 with Vite plugin
- [ ] Configure TypeScript strict mode

### T10.2: Create API client
- [ ] Create `src/api/client.ts` with Axios instance + interceptors
- [ ] Token stored in module-level variable (in-memory)
- [ ] 401 interceptor: refresh token, retry
- [ ] Create all API module files (auth, students, enrollments, documents, notifications, parent)

### T10.3: Create Auth context
- [ ] Create `src/context/AuthContext.tsx`
- [ ] Login, logout, refresh, getCurrentUser
- [ ] Persist auth state across page reloads (silent refresh via cookie)

### T10.4: Create UI components
- [ ] Button, Card, Input, Table, Modal, Badge, Pagination, Sidebar, Layout
- [ ] All styled with Tailwind

### T10.5: Create LoginPage
- [ ] Email/password form
- [ ] "Entrar com Google" button
- [ ] Google OAuth callback handler

### T10.6: Create AdminLayout
- [ ] Sidebar navigation: Dashboard, Alunos, Matrículas, Documentos, Notificações
- [ ] Header with user info + logout

### T10.7: Create AdminDashboard
- [ ] 4 stat cards fetching from API

### T10.8: Create StudentList + StudentForm + StudentDetail
- [ ] Table with search + filters + pagination
- [ ] Create/Edit modal or page
- [ ] Detail page with linked parents

### T10.9: Create EnrollmentList + EnrollmentDetail
- [ ] Filters by period + status
- [ ] Detail with document checklist
- [ ] Approve/Reject buttons

### T10.10: Create DocumentVerification
- [ ] Tabs: Pending / All
- [ ] Approve/Reject with reason
- [ ] File preview for images

### T10.11: Create NotificationCreate + NotificationList
- [ ] Form: title, message, type, target
- [ ] Broadcast or specific users

### T10.12: Create ParentLayout + ParentDashboard
- [ ] Children cards
- [ ] Notification badge

### T10.13: Create ChildDetail + ChildDocuments
- [ ] Tabs: Info, Documents, Grades, History
- [ ] Document upload

### T10.14: Create NotificationInbox
- [ ] Read/unread list
- [ ] Mark read, mark all read

### T10.15: PWA configuration
- [ ] Configure vite-plugin-pwa
- [ ] Create manifest.json
- [ ] Add icons (192x192, 512x512)
- [ ] Test offline caching

### T10.16: Verify
- [ ] `npm run build` — zero errors
- [ ] Login flow works
- [ ] Admin CRUD works
- [ ] Parent portal shows linked children
- [ ] PWA install prompt on mobile
- [ ] Offline page shown when no connection
