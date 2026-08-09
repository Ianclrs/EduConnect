import { useState, useEffect, type ReactNode } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { Button } from './Button';
import { Badge } from './Badge';
import { Menu } from 'lucide-react';

interface SidebarItem {
  label: string;
  path: string;
  icon: ReactNode;
  color?: 'blue' | 'green' | 'amber' | 'red' | 'violet';
}

const colorMap: Record<string, { active: string; hover: string }> = {
  blue:   { active: 'bg-blue-50 text-blue-700',     hover: 'hover:bg-blue-50 hover:text-blue-700' },
  green:  { active: 'bg-green-50 text-green-700',   hover: 'hover:bg-green-50 hover:text-green-700' },
  amber:  { active: 'bg-amber-50 text-amber-700',   hover: 'hover:bg-amber-50 hover:text-amber-700' },
  red:    { active: 'bg-red-50 text-red-700',       hover: 'hover:bg-red-50 hover:text-red-700' },
  violet: { active: 'bg-violet-50 text-violet-700', hover: 'hover:bg-violet-50 hover:text-violet-700' },
};

interface SidebarProps {
  items: SidebarItem[];
  title: string;
  children: ReactNode;
  notificationCount?: number;
}

export function Sidebar({ items, title, children, notificationCount }: SidebarProps) {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [desktopCollapsed, setDesktopCollapsed] = useState(true);
  const location = useLocation();
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  // Fecha mobile ao trocar de rota
  useEffect(() => {
    setMobileOpen(false);
  }, [location.pathname]);

  // Fecha com tecla Escape
  useEffect(() => {
    const handleKey = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        setMobileOpen(false);
        setDesktopCollapsed(true);
      }
    };
    document.addEventListener('keydown', handleKey);
    return () => document.removeEventListener('keydown', handleKey);
  }, []);

  // Bloqueia scroll do body quando overlay mobile está aberto
  useEffect(() => {
    document.body.style.overflow = mobileOpen ? 'hidden' : '';
    return () => { document.body.style.overflow = ''; };
  }, [mobileOpen]);

  const handleLogout = async () => {
    await logout();
    navigate('/login');
  };

  const toggle = () => {
    if (window.innerWidth < 768) {
      setMobileOpen((prev) => !prev);
    } else {
      setDesktopCollapsed((prev) => !prev);
    }
  };

  const isActive = (path: string) => {
    if (path === '/admin' || path === '/parent') {
      return location.pathname === path;
    }
    return location.pathname.startsWith(path);
  };

  const navContent = (
    <>
      <nav className="flex-1 p-3 space-y-0.5 overflow-y-auto">
        {items.map((item) => {
          const colors = colorMap[item.color ?? 'violet'];
          const active = isActive(item.path);
          return (
          <Link
            key={item.path}
            to={item.path}
            className={`flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-medium transition-colors ${
              active ? colors.active : `text-stone-600 ${colors.hover}`
            }`}
          >
            <span className="shrink-0">{item.icon}</span>
            <span className="truncate">{item.label}</span>
            {item.label === 'Notificações' && notificationCount && notificationCount > 0 && (
              <Badge variant="danger" className="ml-auto">{String(notificationCount)}</Badge>
            )}
          </Link>
          )
        })}
      </nav>
      <div className="p-4 border-t border-stone-200 shrink-0">
        <div className="flex items-center gap-3 mb-3">
          <div className="w-8 h-8 rounded-full bg-stone-300 flex items-center justify-center text-stone-700 font-semibold text-sm shrink-0">
            {user?.name?.[0]?.toUpperCase()}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-stone-800 truncate">{user?.name}</p>
            <p className="text-xs text-stone-500">{user?.role}</p>
          </div>
        </div>
        <Button variant="ghost" size="sm" className="w-full justify-center bg-white text-stone-700 shadow-sm hover:bg-red-100 hover:text-red-700 font-medium transition-all" onClick={handleLogout}>
          Sair
        </Button>
      </div>
    </>
  );

  return (
    <div className="flex flex-col h-screen bg-stone-50 overflow-hidden">
      {/* ===== TOP BAR — full width ===== */}
      <div className="sticky top-0 z-20 flex items-center h-14 px-4 bg-stone-200/95 backdrop-blur-sm shadow-lg shrink-0">
        <button
          onClick={toggle}
          className="p-2 -ml-2 rounded-xl hover:bg-stone-300/80 active:scale-95 transition-all"
          aria-label={desktopCollapsed ? 'Expandir menu' : 'Colapsar menu'}
        >
          <Menu size={22} />
        </button>
        <span className="ml-3 font-semibold text-stone-800 truncate text-sm">{title}</span>
      </div>

      {/* ===== BODY: sidebar + content ===== */}
      <div className="flex flex-1 overflow-hidden">
        {/* ===== DESKTOP SIDEBAR ===== */}
        <aside
          className={`hidden md:flex md:flex-col bg-stone-200 shadow-lg transition-all duration-300 ease-out overflow-hidden ${
            desktopCollapsed ? 'md:w-0' : 'md:w-64'
          }`}
        >
          <div className="w-64 flex flex-col h-full">{navContent}</div>
        </aside>

        {/* ===== MOBILE BACKDROP ===== */}
        <div
          className={`fixed top-14 inset-x-0 bottom-0 z-40 bg-black/50 transition-opacity duration-300 ease-out md:hidden ${
            mobileOpen ? 'opacity-100' : 'opacity-0 pointer-events-none'
          }`}
          onClick={() => setMobileOpen(false)}
          aria-hidden="true"
        />

        {/* ===== MOBILE SIDEBAR OVERLAY ===== */}
        <aside
          className={`fixed top-14 bottom-0 left-0 z-50 w-72 max-w-[85vw] bg-stone-200 flex flex-col shadow-lg transform transition-transform duration-300 ease-out md:hidden ${
            mobileOpen ? 'translate-x-0' : '-translate-x-full'
          }`}
        >
          {navContent}
        </aside>

        {/* ===== MAIN CONTENT ===== */}
        <main className="flex-1 overflow-auto">
          <div className="p-4 md:p-6">
            {children}
          </div>
        </main>
      </div>
    </div>
  );
}
