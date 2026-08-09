import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { Card } from '../../components/Card';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { Pagination } from '../../components/Pagination';
import { CheckCheck } from 'lucide-react';
import * as api from '../../api/notifications';
import type { Notification } from '../../types';

export default function NotificationInbox() {
  const [items, setItems] = useState<Notification[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [unreadOnly, setUnreadOnly] = useState(false);

  const load = async (p: number) => {
    try {
      const res = await api.getNotifications({ unreadOnly: unreadOnly || undefined, page: p, pageSize: 10 });
      setItems(res.items); setTotal(res.total); setPage(p);
    } catch { /* ignore */ }
  };
  useEffect(() => { load(1); }, [unreadOnly]);

  const handleMarkRead = async (notificationId: string) => {
    try {
      await api.markRead(notificationId);
      setItems(items.map((n) => n.userNotificationId === notificationId ? { ...n, isRead: true } : n));
    } catch { toast.error('Erro.'); }
  };

  const handleMarkAll = async () => {
    try {
      const res = await api.markAllRead();
      toast.success(`${res.updatedCount} marcadas como lidas.`);
      load(page);
    } catch { toast.error('Erro.'); }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Notificações</h2>
        <div className="flex gap-2">
          <Button variant={unreadOnly ? 'primary' : 'secondary'} size="sm" onClick={() => setUnreadOnly(!unreadOnly)}>Não lidas</Button>
          <Button variant="ghost" size="sm" onClick={handleMarkAll}><CheckCheck size={14} className="mr-1" /> Marcar todas</Button>
        </div>
      </div>
      <Card>
        {items.length === 0 ? <p className="text-gray-500 text-center py-4">Nenhuma notificação.</p> : (
          <ul className="divide-y">
            {items.map((n) => (
              <li key={n.userNotificationId} className={`py-3 cursor-pointer ${n.isRead ? 'opacity-60' : ''}`} onClick={() => !n.isRead && handleMarkRead(n.userNotificationId)}>
                <div className="flex items-start justify-between">
                  <div>
                    <p className="text-sm font-medium">{n.titulo}</p>
                    <p className="text-sm text-gray-500 mt-1">{n.mensagem}</p>
                    <p className="text-xs text-gray-400 mt-1">{new Date(n.createdAt).toLocaleString()}</p>
                  </div>
                  <div className="flex items-center gap-2">
                    {!n.isRead && <span className="w-2 h-2 rounded-full bg-indigo-600" />}
                    <Badge>{n.tipo}</Badge>
                  </div>
                </div>
              </li>
            ))}
          </ul>
        )}
        <Pagination page={page} pageSize={10} total={total} onPageChange={load} />
      </Card>
    </div>
  );
}
