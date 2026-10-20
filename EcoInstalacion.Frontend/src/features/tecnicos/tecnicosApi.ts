import { request } from '../../services/httpClient';
import type { PagedResponse } from '../../types/pagination';
import type {
  TecnicoListItem,
  TecnicoDetail,
  TecnicoCreatePayload,
  TecnicoUpdatePayload,
  TecnicoPagedQuery
} from './types';

export function getTecnicosPaginados(
  query: TecnicoPagedQuery = {}
): Promise<PagedResponse<TecnicoListItem>> {
  const params = new URLSearchParams();
  if (query.q && query.q.trim() !== '') {
    params.append('q', query.q.trim());
  }
  if (query.page !== undefined && query.page !== null) {
    params.append('page', query.page.toString());
  }
  if (query.pageSize !== undefined && query.pageSize !== null) {
    params.append('pageSize', query.pageSize.toString());
  }

  const queryString = params.toString();
  const url = `api/tecnicos/paginados${queryString ? `?${queryString}` : ''}`;

  return request<PagedResponse<TecnicoListItem>>(url);
}

export function getTecnicos(): Promise<TecnicoListItem[]> {
  return request<TecnicoListItem[]>('api/tecnicos');
}

export function getTecnicoById(id: number): Promise<TecnicoDetail> {
  return request<TecnicoDetail>(`api/tecnicos/${id}`);
}

export function createTecnico(payload: TecnicoCreatePayload): Promise<TecnicoDetail> {
  return request<TecnicoDetail>('api/tecnicos', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function updateTecnico(
  id: number,
  payload: TecnicoUpdatePayload
): Promise<TecnicoDetail> {
  return request<TecnicoDetail>(`api/tecnicos/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function deleteTecnico(id: number): Promise<void> {
  return request<void>(`api/tecnicos/${id}`, { method: 'DELETE' });
}
