import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Card } from '../../components/Card';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { Pagination } from '../../components/Pagination';
import { Modal } from '../../components/Modal';
import { Input } from '../../components/Input';
import { Table } from '../../components/Table';
import * as api from '../../api/enrollments';
import type { Enrollment, EnrollmentPeriod } from '../../types';

export default function EnrollmentList() {
  const [enrollments, setEnrollments] = useState<Enrollment[]>([]);
  const [periods, setPeriods] = useState<EnrollmentPeriod[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [periodId, setPeriodId] = useState('');
  const [statusFilter, setStatusFilter] = useState('');
  const [rejectId, setRejectId] = useState('');
  const [motivo, setMotivo] = useState('');

  const load = async (p: number) => {
    try {
      const [res, per] = await Promise.all([
        api.getEnrollments({ periodId: periodId || undefined, status: statusFilter || undefined, page: p, pageSize: 10 }),
        api.getEnrollmentPeriods(),
      ]);
      setEnrollments(res.items); setTotal(res.total); setPeriods(per); setPage(p);
    } catch { toast.error('Erro.'); }
  };
  useEffect(() => { load(1); }, [periodId, statusFilter]);

  const handleApprove = async (id: string) => { try { await api.approveEnrollment(id); toast.success('Aprovada!'); load(page); } catch { toast.error('Erro.'); } };
  const handleReject = async () => { try { await api.rejectEnrollment(rejectId, motivo); toast.success('Rejeitada.'); setRejectId(''); load(page); } catch { toast.error('Erro.'); } };

  const statusV = (s: string) => s === 'Aprovado' ? 'success' : s === 'Pendente' ? 'warning' : s === 'Rejeitado' ? 'danger' : 'info';

  const columns = [
    { header: 'Aluno', accessor: (e: Enrollment) => <Link to={`/admin/enrollments/${e.id}`} className="text-indigo-600 hover:underline">{e.studentName}</Link> },
    { header: 'Período', accessor: (e: Enrollment) => e.periodName },
    { header: 'Status', accessor: (e: Enrollment) => <Badge variant={statusV(e.status)}>{e.status}</Badge> },
    { header: '', accessor: (e: Enrollment) => e.status === 'Pendente' ? (
      <div className="flex justify-end gap-1">
        <Button variant="ghost" size="sm" onClick={() => handleApprove(e.id)}>Aprovar</Button>
        <Button variant="ghost" size="sm" onClick={() => setRejectId(e.id)}>Rejeitar</Button>
      </div>
    ) : null, className: 'text-right' },
  ];

  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Matrículas</h2>
      <Card>
        <div className="flex flex-wrap gap-3 mb-4">
          <select className="border rounded-lg px-3 py-2 text-sm" value={periodId} onChange={(e) => setPeriodId(e.target.value)}>
            <option value="">Todos períodos</option>
            {periods.map((p) => <option key={p.id} value={p.id}>{p.nome} ({p.anoLetivo})</option>)}
          </select>
          <select className="border rounded-lg px-3 py-2 text-sm" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value)}>
            <option value="">Todos status</option>
            <option value="Pendente">Pendente</option>
            <option value="Aprovado">Aprovado</option>
            <option value="Rejeitado">Rejeitado</option>
          </select>
        </div>
        <Table columns={columns} data={enrollments} keyExtractor={(e) => e.id} />
        <Pagination page={page} pageSize={10} total={total} onPageChange={load} />
      </Card>
      <Modal open={!!rejectId} onClose={() => setRejectId('')} title="Rejeitar Matrícula" onConfirm={handleReject} confirmLabel="Rejeitar" confirmVariant="danger">
        <Input label="Motivo da rejeição" value={motivo} onChange={(e) => setMotivo(e.target.value)} />
      </Modal>
    </div>
  );
}
