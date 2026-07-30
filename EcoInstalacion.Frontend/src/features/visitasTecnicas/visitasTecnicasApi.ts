import { request } from '../../services/httpClient';
import type {
  VisitaTecnicaCreatePayload,
  VisitaTecnicaDetail,
  VisitaTecnicaListItem,
  VisitaTecnicaUpdatePayload,
  PagedResult,
  VisitaTecnicaPagedQuery
} from './types';

export function getVisitasTecnicasPaginadas(
  query: VisitaTecnicaPagedQuery
): Promise<PagedResult<VisitaTecnicaListItem>> {
  const params = new URLSearchParams();
  if (query.q && query.q.trim() !== '') {
    params.append('q', query.q.trim());
  }
  if (query.aprobado !== undefined && query.aprobado !== null) {
    params.append('aprobado', query.aprobado.toString());
  }
  if (query.solicitudId !== undefined && query.solicitudId !== null) {
    params.append('solicitudId', query.solicitudId.toString());
  }
  if (query.estadoSolicitud && query.estadoSolicitud.trim() !== '') {
    params.append('estadoSolicitud', query.estadoSolicitud.trim());
  }
  if (query.clienteNombre && query.clienteNombre.trim() !== '') {
    params.append('clienteNombre', query.clienteNombre.trim());
  }
  if (query.tecnicoNombre && query.tecnicoNombre.trim() !== '') {
    params.append('tecnicoNombre', query.tecnicoNombre.trim());
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
  if (query.pageNumber !== undefined && query.pageNumber !== null) {
    params.append('pageNumber', query.pageNumber.toString());
  }
  if (query.pageSize !== undefined && query.pageSize !== null) {
    params.append('pageSize', query.pageSize.toString());
  }

  const queryString = params.toString();
  const url = `api/visitas-tecnicas/paginadas${queryString ? `?${queryString}` : ''}`;

  return request<PagedResult<VisitaTecnicaListItem>>(url);
}

export function getVisitasTecnicas(): Promise<VisitaTecnicaListItem[]> {
  return request<VisitaTecnicaListItem[]>('api/visitas-tecnicas');
}

export function getVisitasTecnicasBySolicitudId(solicitudId: number): Promise<VisitaTecnicaListItem[]> {
  return request<VisitaTecnicaListItem[]>(`api/visitas-tecnicas?solicitudId=${solicitudId}`);
}

export function getVisitaTecnicaById(id: number): Promise<VisitaTecnicaDetail> {
  return request<VisitaTecnicaDetail>(`api/visitas-tecnicas/${id}`);
}

export function createVisitaTecnica(
  payload: VisitaTecnicaCreatePayload
): Promise<VisitaTecnicaDetail> {
  return request<VisitaTecnicaDetail>('api/visitas-tecnicas', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function updateVisitaTecnica(
  id: number,
  payload: VisitaTecnicaUpdatePayload
): Promise<VisitaTecnicaDetail> {
  return request<VisitaTecnicaDetail>(`api/visitas-tecnicas/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function deleteVisitaTecnica(id: number): Promise<void> {
  return request<void>(`api/visitas-tecnicas/${id}`, { method: 'DELETE' });
}
