import { request } from '../../services/httpClient';
import type {
  TecnicoListItem,
  TecnicoDetail,
  TecnicoCreatePayload,
  TecnicoUpdatePayload
} from './types';

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
