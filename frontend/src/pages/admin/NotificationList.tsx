import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Card } from '../../components/Card';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { Pagination } from '../../components/Pagination';
import { Plus } from 'lucide-react';
import * as api from '../../api/notifications';
import type { Notification } from '../../types';

export default function NotificationList() {
  const [items, setItems] = useState<Notification[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);

  const load = async (p: number) => {
    try {
      const res = await api.getNotifications({ page: p, pageSize: 10 });
      setItems(res.items); setTotal(res.total); setPage(p);
    } catch { /* ignore */ }
  };
  useEffect(() => { load(1); }, []);

  return (
    <div>
      <div className="flex items-center justify-between mb-6 gap-3">
        <h2 className="text-2xl font-bold text-gray-900">Notificações</h2>
        <Link to="/admin/notifications/create"><Button className="shrink-0"><Plus size={16} className="mr-1" /> Nova</Button></Link>
      </div>
      <Card>
        {items.length === 0 ? <p className="text-gray-500 text-center py-4">Nenhuma notificação.</p> : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead><tr className="border-b text-left"><th className="py-2 whitespace-nowrap">Título</th><th className="py-2 whitespace-nowrap">Mensagem</th><th className="py-2 whitespace-nowrap">Tipo</th><th className="py-2 whitespace-nowrap">Data</th></tr></thead>
              <tbody>
                {items.map((n) => (
                  <tr key={n.id} className="border-b">
                    <td className="py-2 font-medium whitespace-nowrap">{n.titulo}</td>
                    <td className="py-2 text-gray-500 truncate max-w-xs">{n.mensagem}</td>
                    <td className="py-2 whitespace-nowrap"><Badge>{n.tipo}</Badge></td>
                    <td className="py-2 text-gray-400 whitespace-nowrap">{new Date(n.createdAt).toLocaleDateString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
        <Pagination page={page} pageSize={10} total={total} onPageChange={load} />
      </Card>
    </div>
  );
}
