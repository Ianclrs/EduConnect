import { useEffect, useState } from 'react';
import { Card } from '../../components/Card';
import { Users, GraduationCap, FileText, Bell } from 'lucide-react';
import * as studentsApi from '../../api/students';
import * as enrollmentsApi from '../../api/enrollments';
import * as documentsApi from '../../api/documents';
import * as notificationsApi from '../../api/notifications';

export default function AdminDashboard() {
  const [stats, setStats] = useState({ students: 0, enrollments: 0, documents: 0, notifications: 0 });

  useEffect(() => {
    (async () => {
      const [s, e, d, n] = await Promise.all([
        studentsApi.getStudents({ pageSize: 1 }).then(r => r.total).catch(() => 0),
        enrollmentsApi.getEnrollments({ pageSize: 1 }).then(r => r.total).catch(() => 0),
        documentsApi.getPendingDocuments({ pageSize: 1 }).then(r => r.total).catch(() => 0),
        notificationsApi.getUnreadCount().then(r => r.count).catch(() => 0),
      ]);
      setStats({ students: s, enrollments: e, documents: d, notifications: n });
    })();
  }, []);

  const cards = [
    { label: 'Total de Alunos', value: stats.students, icon: Users, color: 'text-blue-600 bg-blue-100' },
    { label: 'Matrículas', value: stats.enrollments, icon: GraduationCap, color: 'text-green-600 bg-green-100' },
    { label: 'Docs Pendentes', value: stats.documents, icon: FileText, color: 'text-yellow-600 bg-yellow-100' },
    { label: 'Não Lidas', value: stats.notifications, icon: Bell, color: 'text-red-600 bg-red-100' },
  ];

  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h2>
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        {cards.map((c) => (
          <Card key={c.label}>
            <div className="flex items-center gap-4">
              <div className={`p-3 rounded-lg ${c.color}`}><c.icon size={24} /></div>
              <div>
                <p className="text-sm text-gray-500">{c.label}</p>
                <p className="text-2xl font-bold text-gray-900">{c.value}</p>
              </div>
            </div>
          </Card>
        ))}
      </div>
    </div>
  );
}
