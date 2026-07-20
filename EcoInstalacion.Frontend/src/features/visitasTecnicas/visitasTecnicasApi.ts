import { request } from '../../services/httpClient';
import type {
  VisitaTecnicaCreatePayload,
  VisitaTecnicaDetail,
  VisitaTecnicaListItem,
  VisitaTecnicaUpdatePayload
} from './types';

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
