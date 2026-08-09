import api from './client';
import type { Document, PagedResponse, DocumentType } from '../types';

export async function getPendingDocuments(params: {
  page?: number;
  pageSize?: number;
}): Promise<PagedResponse<Document>> {
  const res = await api.get('/documents/pending', { params });
  return res.data;
}

export async function getStudentDocuments(studentId: string, params: {
  page?: number;
  pageSize?: number;
}): Promise<PagedResponse<Document>> {
  const res = await api.get(`/documents/student/${studentId}`, { params });
  return res.data;
}

export async function uploadDocument(formData: FormData): Promise<Document> {
  const res = await api.post('/documents/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return res.data;
}

export async function verifyDocument(id: string, approved: boolean, motivoRejeicao?: string): Promise<void> {
  await api.put(`/documents/${id}/verify`, { approved, motivoRejeicao });
}

export async function getDocumentTypes(): Promise<DocumentType[]> {
  const res = await api.get('/document-types');
  return res.data;
}

export async function getDocumentDownloadUrl(id: string): Promise<string> {
  return `${api.defaults.baseURL}/documents/${id}/download`;
}
