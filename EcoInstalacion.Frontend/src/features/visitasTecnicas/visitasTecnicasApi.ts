import { request } from '../../services/httpClient';
import type { PagedResponse } from '../../types/pagination';
import type {
  VisitaTecnicaCreatePayload,
  VisitaTecnicaDetail,
  VisitaTecnicaListItem,
  VisitaTecnicaUpdatePayload,
  VisitaTecnicaPagedQuery
} from './types';

export function getVisitasTecnicasPaginadas(
  query: VisitaTecnicaPagedQuery
): Promise<PagedResponse<VisitaTecnicaListItem>> {
  const params = new URLSearchParams();
  if (query.q && query.q.trim() !== '') {
    params.append('q', query.q.trim());
  }
  if (query.aprobado !== undefined && query.aprobado !== null) {
    params.append('aprobado', query.aprobado.toString());
  }
  if (query.page !== undefined && query.page !== null) {
    params.append('page', query.page.toString());
  }
  if (query.pageSize !== undefined && query.pageSize !== null) {
    params.append('pageSize', query.pageSize.toString());
  }

  const queryString = params.toString();
  const url = `api/visitas-tecnicas/paginadas${queryString ? `?${queryString}` : ''}`;

  return request<PagedResponse<VisitaTecnicaListItem>>(url);
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
