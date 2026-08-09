import api from './client';
import type { Notification, PagedResponse } from '../types';

export interface CreateNotificationRequest {
  titulo: string;
  mensagem: string;
  tipo: number;
  referenceId?: string;
  userIds?: string[];
}

export interface BroadcastNotificationRequest {
  titulo: string;
  mensagem: string;
  tipo: number;
  referenceId?: string;
}

export async function getNotifications(params: {
  unreadOnly?: boolean;
  page?: number;
  pageSize?: number;
}): Promise<PagedResponse<Notification>> {
  const res = await api.get('/notifications', { params });
  return res.data;
}

export async function createNotification(data: CreateNotificationRequest): Promise<Notification> {
  const res = await api.post('/notifications', data);
  return res.data;
}

export async function broadcastNotification(data: BroadcastNotificationRequest): Promise<{ recipientCount: number }> {
  const res = await api.post('/notifications/broadcast', data);
  return res.data;
}

export async function sendByStudent(studentId: string, titulo: string, mensagem: string, tipo: number): Promise<{ recipientCount: number }> {
  const res = await api.post(`/notifications/by-student/${studentId}`, { titulo, mensagem, tipo });
  return res.data;
}

export async function markRead(id: string): Promise<void> {
  await api.put(`/notifications/${id}/read`);
}

export async function markAllRead(): Promise<{ updatedCount: number }> {
  const res = await api.put('/notifications/read-all');
  return res.data;
}

export async function getUnreadCount(): Promise<{ count: number }> {
  const res = await api.get('/notifications/unread-count');
  return res.data;
}
