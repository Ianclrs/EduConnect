import { useEffect, useState, useRef } from 'react';
import { useParams, Link } from 'react-router-dom';
import toast from 'react-hot-toast';
import { Card } from '../../components/Card';
import { Badge } from '../../components/Badge';
import { Button } from '../../components/Button';
import { ArrowLeft, Upload } from 'lucide-react';
import * as api from '../../api/parent';
import type { Document } from '../../types';

export default function ChildDocuments() {
  const { id } = useParams();
  const [docs, setDocs] = useState<Document[]>([]);
  const [uploading, setUploading] = useState(false);
  const fileRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (id) api.getChildDocuments(id).then(setDocs).catch(() => {});
  }, [id]);

  const handleUpload = async (e: React.FormEvent) => {
    e.preventDefault();
    const file = fileRef.current?.files?.[0];
    if (!file || !id) return;
    if (file.size > 10 * 1024 * 1024) { toast.error('Arquivo muito grande (max 10MB).'); return; }
    setUploading(true);
    try {
      const formData = new FormData();
      formData.append('file', file);
      formData.append('documentTypeId', '00000000-0000-0000-0000-000000000001');
      const doc = await api.uploadChildDocument(id, formData);
      toast.success('Upload concluído!');
      setDocs([doc, ...docs]);
      if (fileRef.current) fileRef.current.value = '';
    } catch { toast.error('Erro no upload.'); }
    finally { setUploading(false); }
  };

  return (
    <div>
      <Link to={`/parent/children/${id}`} className="flex items-center gap-1 text-sm text-gray-500 mb-4"><ArrowLeft size={16} /> Voltar</Link>
      <h2 className="text-2xl font-bold text-gray-900 mb-6">Documentos</h2>
      <Card className="mb-6">
        <form onSubmit={handleUpload} className="flex flex-col sm:flex-row gap-4 items-stretch sm:items-end">
          <input type="file" ref={fileRef} className="text-sm" accept=".pdf,.jpg,.jpeg,.png" required />
          <Button type="submit" disabled={uploading}><Upload size={14} className="mr-1" />{uploading ? 'Enviando...' : 'Upload'}</Button>
        </form>
      </Card>
      <Card>
        {docs.length === 0 ? <p className="text-gray-500 text-center py-4">Nenhum documento.</p> : (
          <ul className="divide-y">
            {docs.map((d) => (
              <li key={d.id} className="py-3 flex items-center justify-between">
                <div>
                  <p className="text-sm font-medium">{d.nomeArquivo}</p>
                  <p className="text-xs text-gray-500">{d.documentTypeName} • {new Date(d.createdAt).toLocaleDateString()}</p>
                </div>
                <Badge variant={d.status === 'Aprovado' ? 'success' : d.status === 'Pendente' ? 'warning' : 'danger'}>{d.status}</Badge>
              </li>
            ))}
          </ul>
        )}
      </Card>
    </div>
  );
}
