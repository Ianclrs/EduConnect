import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { Card } from '../../components/Card';
import { Badge } from '../../components/Badge';
import { Users } from 'lucide-react';
import * as api from '../../api/parent';
import * as notifApi from '../../api/notifications';
import type { ChildSummary } from '../../types';

export default function ParentDashboard() {
  const [dashboard, setDashboard] = useState<{ totalChildren: number; unreadNotifications: number; pendingDocuments: number; activeEnrollments: number; children: ChildSummary[] } | null>(null);

  useEffect(() => {
    Promise.all([api.getDashboard(), notifApi.getUnreadCount()]).then(([d, n]) => {
      setDashboard({ ...d, unreadNotifications: n.count });
    }).catch(() => {});
  }, []);

  if (!dashboard) return <p>Carregando...</p>;

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Meus Filhos</h2>
        {dashboard.unreadNotifications > 0 && <Badge variant="danger">{`${dashboard.unreadNotifications} não lidas`}</Badge>}
      </div>
      {dashboard.children.length === 0 ? (
        <Card><p className="text-gray-500 text-center">Nenhum filho vinculado.</p></Card>
      ) : (
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
          {dashboard.children.map((c) => (
            <Link key={c.studentId} to={`/parent/children/${c.studentId}`}>
              <Card className="hover:shadow-md transition-shadow cursor-pointer h-full">
                <div className="flex items-center gap-3 mb-3">
                  <div className="p-2 rounded-lg bg-indigo-100 text-indigo-600"><Users size={20} /></div>
                  <div>
                    <h3 className="font-semibold text-gray-900">{c.nome}</h3>
                    <p className="text-sm text-gray-500">{c.turma} • {c.anoLetivo}</p>
                  </div>
                </div>
                <div className="flex gap-2 flex-wrap">
                  {c.enrollmentStatus && <Badge variant="success">{c.enrollmentStatus}</Badge>}
                  {c.pendingDocuments > 0 && <Badge variant="warning">{`${c.pendingDocuments} doc(s) pendente(s)`}</Badge>}
                </div>
              </Card>
            </Link>
          ))}
        </div>
      )}
    </div>
  );
}
