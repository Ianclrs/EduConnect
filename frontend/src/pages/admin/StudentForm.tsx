import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Button } from '../../components/Button';
import { Input } from '../../components/Input';
import { Card } from '../../components/Card';
import * as api from '../../api/students';

export default function StudentForm() {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEdit = !!id;
  const [form, setForm] = useState({ nome: '', turma: '', anoLetivo: new Date().getFullYear(), dataNascimento: '', cpf: '', observacoes: '' });
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (id) { api.getStudent(id).then(s => setForm({ nome: s.nome, turma: s.turma, anoLetivo: s.anoLetivo, dataNascimento: s.dataNascimento?.split('T')[0] || '', cpf: s.cpf || '', observacoes: s.observacoes || '' })).catch(() => navigate('/admin/students')); }
  }, [id, navigate]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      const data = { ...form, cpf: form.cpf || undefined, observacoes: form.observacoes || undefined };
      if (isEdit) { await api.updateStudent(id!, data); toast.success('Aluno atualizado!'); }
      else { await api.createStudent(data); toast.success('Aluno criado!'); }
      navigate('/admin/students');
    } catch { toast.error('Erro ao salvar.'); }
    finally { setLoading(false); }
  };

  return (
    <div>
      <h2 className="text-2xl font-bold text-gray-900 mb-6">{isEdit ? 'Editar Aluno' : 'Novo Aluno'}</h2>
      <Card>
        <form onSubmit={handleSubmit} className="space-y-4 max-w-lg">
          <Input label="Nome" value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required />
          <Input label="Data de Nascimento" type="date" value={form.dataNascimento} onChange={(e) => setForm({ ...form, dataNascimento: e.target.value })} required />
          <Input label="CPF" value={form.cpf} onChange={(e) => setForm({ ...form, cpf: e.target.value })} />
          <Input label="Turma" value={form.turma} onChange={(e) => setForm({ ...form, turma: e.target.value })} required />
          <Input label="Ano Letivo" type="number" value={String(form.anoLetivo)} onChange={(e) => setForm({ ...form, anoLetivo: Number(e.target.value) })} required />
          <Input label="Observações" value={form.observacoes} onChange={(e) => setForm({ ...form, observacoes: e.target.value })} />
          <div className="flex gap-3">
            <Button type="submit" disabled={loading}>{loading ? 'Salvando...' : 'Salvar'}</Button>
            <Button variant="secondary" type="button" onClick={() => navigate('/admin/students')}>Cancelar</Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
