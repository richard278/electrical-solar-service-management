import { request } from '../../services/httpClient';
import type {
  SolicitudCreatePayload,
  SolicitudDetail,
  SolicitudListItem,
  SolicitudUpdatePayload,
  PagedResult,
  SolicitudPagedQuery
} from './types';

export function getSolicitudesPaginadas(
  query: SolicitudPagedQuery
): Promise<PagedResult<SolicitudListItem>> {
  const params = new URLSearchParams();
  if (query.q && query.q.trim() !== '') {
    params.append('q', query.q.trim());
  }
  if (query.estado && query.estado.trim() !== '' && query.estado !== 'Todas') {
    params.append('estado', query.estado.trim());
  }
  if (query.clienteId !== undefined && query.clienteId !== null) {
    params.append('clienteId', query.clienteId.toString());
  }
  if (query.tecnicoAsignadoId !== undefined && query.tecnicoAsignadoId !== null) {
    params.append('tecnicoAsignadoId', query.tecnicoAsignadoId.toString());
  }
  if (query.tipoInstalacion && query.tipoInstalacion.trim() !== '') {
    params.append('tipoInstalacion', query.tipoInstalacion.trim());
  }
  if (query.fechaDesde && query.fechaDesde.trim() !== '') {
    params.append('fechaDesde', query.fechaDesde.trim());
  }
  if (query.fechaHasta && query.fechaHasta.trim() !== '') {
    params.append('fechaHasta', query.fechaHasta.trim());
  }
  if (query.clienteNombre && query.clienteNombre.trim() !== '') {
    params.append('clienteNombre', query.clienteNombre.trim());
  }
  if (query.tecnicoNombre && query.tecnicoNombre.trim() !== '') {
    params.append('tecnicoNombre', query.tecnicoNombre.trim());
  }
  if (query.pageNumber !== undefined && query.pageNumber !== null) {
    params.append('pageNumber', query.pageNumber.toString());
  }
  if (query.pageSize !== undefined && query.pageSize !== null) {
    params.append('pageSize', query.pageSize.toString());
  }

  const queryString = params.toString();
  const url = `api/solicitudes/paginadas${queryString ? `?${queryString}` : ''}`;

  return request<PagedResult<SolicitudListItem>>(url);
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
