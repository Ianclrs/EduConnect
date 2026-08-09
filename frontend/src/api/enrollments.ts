import api from './client';
import type { Enrollment, EnrollmentPeriod, PagedResponse } from '../types';

export async function getEnrollments(params: {
  periodId?: string;
  status?: string;
  page?: number;
  pageSize?: number;
}): Promise<PagedResponse<Enrollment>> {
  const res = await api.get('/enrollments', { params });
  return res.data;
}

export async function getEnrollment(id: string): Promise<Enrollment> {
  const res = await api.get(`/enrollments/${id}`);
  return res.data;
}

export async function approveEnrollment(id: string): Promise<void> {
  await api.put(`/enrollments/${id}/approve`);
}

export async function rejectEnrollment(id: string, motivo: string): Promise<void> {
  await api.put(`/enrollments/${id}/reject`, { motivoRejeicao: motivo });
}

export async function getEnrollmentPeriods(): Promise<EnrollmentPeriod[]> {
  const res = await api.get('/enrollment-periods');
  return res.data;
}
