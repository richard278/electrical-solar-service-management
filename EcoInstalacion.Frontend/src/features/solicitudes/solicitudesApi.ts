import { request } from '../../services/httpClient';
import type {
  SolicitudCreatePayload,
  SolicitudDetail,
  SolicitudListItem,
  SolicitudUpdatePayload
} from './types';

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
