# Spec 100: Design — Frontend Application (React + PWA)

## Design Approach

SPA React com **Vite 6 + Tailwind CSS 4 + TypeScript strict**. Arquitetura de rotas com duas áreas protegidas (`/admin` para Admin/Staff, `/parent` para Parents). Estado de auth mantido em `AuthContext` com token em memória (variável de módulo). PWA via `vite-plugin-pwa` com estratégia de cache NetworkFirst para API.

## Architecture Decisions

- **AD-001: Token em memória** — access token em variável de módulo (não localStorage/Redux) previne XSS.
- **AD-002: Axios interceptor para refresh** — 401 → tenta refresh → retry. Falha → redirect /login.
- **AD-003: PWA autoUpdate** — service worker atualiza automaticamente, sem prompt de confirmação.

## Project Structure
```
frontend/
├── index.html
├── package.json, tsconfig.json, vite.config.ts, tailwind.config.ts
├── public/
│   ├── manifest.json, icon-192.png, icon-512.png, favicon.ico
└── src/
    ├── main.tsx, App.tsx, index.css
    ├── api/          # Axios client + API modules
    ├── hooks/        # useAuth, useApi, usePwa
    ├── context/      # AuthContext
    ├── components/   # ui/ + shared/
    ├── pages/        # LoginPage, admin/*, parent/*
    └── types/        # TypeScript types
```

## Technology Stack

| Package | Version |
|---|---|
| react, react-dom | ^19.0 |
| react-router-dom | ^7.x |
| vite | ^6.x |
| typescript | ^5.7 |
| tailwindcss, @tailwindcss/vite | ^4.x |
| axios | ^1.7 |
| react-hot-toast | ^2.x |
| lucide-react | ^0.400+ |
| vite-plugin-pwa | ^0.21+ |

## Axios Client (src/api/client.ts)
```typescript
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000',
  withCredentials: true,
});

api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (error) => {
    if (error.response?.status === 401 && !error.config._retry) {
      error.config._retry = true;
      const newToken = await refreshAccessToken();
      setAccessToken(newToken);
      error.config.headers.Authorization = `Bearer ${newToken}`;
      return api(error.config);
    }
    return Promise.reject(error);
  }
);
```

## Routing (App.tsx)
```tsx
<Routes>
  <Route path="/login" element={<LoginPage />} />
  <Route path="/auth/google/callback" element={<GoogleCallback />} />
  <Route path="/admin" element={<ProtectedRoute roles={['Admin','Staff']}><AdminLayout /></ProtectedRoute>}>
    <Route index element={<AdminDashboard />} />
    <Route path="students" element={<StudentList />} />
    <Route path="students/new" element={<StudentForm />} />
    <Route path="students/:id" element={<StudentDetail />} />
    <Route path="students/:id/edit" element={<StudentForm />} />
    <Route path="enrollments" element={<EnrollmentList />} />
    <Route path="enrollments/:id" element={<EnrollmentDetail />} />
    <Route path="documents" element={<DocumentVerification />} />
    <Route path="notifications" element={<NotificationList />} />
    <Route path="notifications/create" element={<NotificationCreate />} />
  </Route>
  <Route path="/parent" element={<ProtectedRoute roles={['Parent']}><ParentLayout /></ProtectedRoute>}>
    <Route index element={<ParentDashboard />} />
    <Route path="children/:id" element={<ChildDetail />} />
    <Route path="children/:id/documents" element={<ChildDocuments />} />
    <Route path="notifications" element={<NotificationInbox />} />
  </Route>
</Routes>
```

## PWA Configuration (vite.config.ts)
```typescript
VitePWA({
  registerType: 'autoUpdate',
  manifest: {
    name: 'EduGestor', short_name: 'EduGestor',
    theme_color: '#4f46e5', background_color: '#f9fafb',
    display: 'standalone', orientation: 'portrait-primary',
    icons: [{ src: '/icon-192.png', sizes: '192x192', type: 'image/png' },
            { src: '/icon-512.png', sizes: '512x512', type: 'image/png' }],
  },
  workbox: {
    globPatterns: ['**/*.{js,css,html,ico,png,svg,woff2}'],
    runtimeCaching: [{
      urlPattern: /^https?:\/\/.*\/api\/.*/i,
      handler: 'NetworkFirst',
      options: { cacheName: 'api-cache', expiration: { maxEntries: 50, maxAgeSeconds: 300 } },
    }],
  },
})
```

## Pages Summary

| Page | Key Features |
|---|---|
| LoginPage | Email/password form + "Entrar com Google" + Google callback handler |
| AdminDashboard | 4 stat cards: Students, Enrollments, Documents, Notifications |
| StudentList | Search + filters + paginated table + create/edit/delete |
| EnrollmentList | Filter by period/status + approve/reject buttons |
| DocumentVerification | Tabs Pending/All + preview + approve/reject modal |
| NotificationCreate | Form with broadcast or specific users target |
| ParentDashboard | Children cards + unread badge |
| ChildDetail | Tabs: Info, Documents, Grades, History + upload |
| NotificationInbox | Read/unread list + mark read/mark all read |

## File / Module Layout

All in `frontend/` at repository root.

## Cross-Reference: Requirements → Design

| Requirement | Covered By |
|---|---|
| FR-001: Scaffold | Project Structure, Technology Stack |
| FR-002: Login | LoginPage, AuthContext |
| FR-003: Axios Client | Axios Client section |
| FR-004: AuthContext | AuthContext |
| FR-005: UI Components | components/ui/ |
| FR-006: AdminLayout | Routing, AdminLayout |
| FR-007-011: Admin pages | Pages Summary table |
| FR-012-014: Parent pages | Pages Summary table |
| FR-015: PWA | PWA Configuration |
