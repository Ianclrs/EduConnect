import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { Card } from '../../components/Card';
import { Pagination } from '../../components/Pagination';
import { Badge } from '../../components/Badge';
import { Table } from '../../components/Table';
import { Plus, Search } from 'lucide-react';
import * as api from '../../api/students';
import type { Student } from '../../types';

export default function StudentList() {
  const [students, setStudents] = useState<Student[]>([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState('');
  const [turma, setTurma] = useState('');
  const [status, setStatus] = useState('');
  const [loading, setLoading] = useState(false);

  const load = async (p: number) => {
    setLoading(true);
    try {
      const res = await api.getStudents({ search: search || undefined, turma: turma || undefined, status: status || undefined, page: p, pageSize: 10 });
      setStudents(res.items); setTotal(res.total); setPage(p);
    } catch { toast.error('Erro ao carregar.'); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(1); }, []);

  const handleDelete = async (id: string) => {
    if (!confirm('Remover este aluno?')) return;
    try { await api.deleteStudent(id); toast.success('Removido.'); load(page); }
    catch { toast.error('Erro.'); }
  };

  const statusVariant = (s: string) => s === 'Ativo' ? 'success' : s === 'Inativo' ? 'warning' : 'info';

  const columns = [
    { header: 'Nome', accessor: (s: Student) => <Link to={`/admin/students/${s.id}`} className="text-indigo-600 hover:underline">{s.nome}</Link> },
    { header: 'Turma', accessor: (s: Student) => s.turma },
    { header: 'Ano', accessor: (s: Student) => String(s.anoLetivo) },
    { header: 'Status', accessor: (s: Student) => <Badge variant={statusVariant(s.status)}>{s.status}</Badge> },
    { header: '', accessor: (s: Student) => (
      <div className="flex justify-end gap-1">
        <Link to={`/admin/students/${s.id}/edit`}><Button variant="ghost" size="sm">Editar</Button></Link>
        <Button variant="ghost" size="sm" onClick={() => handleDelete(s.id)}>Remover</Button>
      </div>
    ), className: 'text-right' },
  ];

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h2 className="text-2xl font-bold text-gray-900">Alunos</h2>
        <Link to="/admin/students/new"><Button><Plus size={16} className="mr-1" /> Novo Aluno</Button></Link>
      </div>
      <Card>
        <div className="flex flex-wrap gap-3 mb-4">
          <div className="flex-1 min-w-[200px] flex gap-2">
            <Input placeholder="Buscar por nome..." value={search} onChange={(e) => setSearch(e.target.value)} />
            <Button variant="secondary" onClick={() => load(1)}><Search size={16} /></Button>
          </div>
          <Input placeholder="Turma" value={turma} onChange={(e) => setTurma(e.target.value)} className="max-w-[150px]" />
          <select className="border rounded-lg px-3 py-2 text-sm" value={status} onChange={(e) => { setStatus(e.target.value); setTimeout(() => load(1), 0); }}>
            <option value="">Todos status</option>
            <option value="Ativo">Ativo</option>
            <option value="Inativo">Inativo</option>
            <option value="Transferido">Transferido</option>
          </select>
        </div>
        {loading ? <p className="text-gray-500">Carregando...</p> : <Table columns={columns} data={students} keyExtractor={(s) => s.id} />}
        <Pagination page={page} pageSize={10} total={total} onPageChange={load} />
      </Card>
    </div>
  );
}
