import { request } from '../../services/httpClient';
import type { PagedResponse } from '../../types/pagination';
import type {
  SolicitudCreatePayload,
  SolicitudDetail,
  SolicitudListItem,
  SolicitudUpdatePayload,
  SolicitudPagedQuery
} from './types';

export function getSolicitudesPaginadas(
  query: SolicitudPagedQuery
): Promise<PagedResponse<SolicitudListItem>> {
  const params = new URLSearchParams();
  if (query.q && query.q.trim() !== '') {
    params.append('q', query.q.trim());
  }
  if (query.estado && query.estado.trim() !== '' && query.estado !== 'Todas') {
    params.append('estado', query.estado.trim());
  }
  if (query.page !== undefined && query.page !== null) {
    params.append('page', query.page.toString());
  }
  if (query.pageSize !== undefined && query.pageSize !== null) {
    params.append('pageSize', query.pageSize.toString());
  }

  const queryString = params.toString();
  const url = `api/solicitudes/paginadas${queryString ? `?${queryString}` : ''}`;

  return request<PagedResponse<SolicitudListItem>>(url);
}

export function getSolicitudes(): Promise<SolicitudListItem[]> {
  return request<SolicitudListItem[]>('api/solicitudes');
}

export function getSolicitudById(id: number): Promise<SolicitudDetail> {
  return request<SolicitudDetail>(`api/solicitudes/${id}`);
}

export function createSolicitud(
  payload: SolicitudCreatePayload
): Promise<SolicitudDetail> {
  return request<SolicitudDetail>('api/solicitudes', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function updateSolicitud(
  id: number,
  payload: SolicitudUpdatePayload
): Promise<SolicitudDetail> {
  return request<SolicitudDetail>(`api/solicitudes/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function deleteSolicitud(id: number): Promise<void> {
  return request<void>(`api/solicitudes/${id}`, { method: 'DELETE' });
}
