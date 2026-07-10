import { request } from '../../services/httpClient';
import type { VisitaTecnicaListItem } from './types';

export function getVisitasTecnicas(): Promise<VisitaTecnicaListItem[]> {
  return request<VisitaTecnicaListItem[]>('api/visitas-tecnicas');
}

export function getVisitasTecnicasBySolicitudId(solicitudId: number): Promise<VisitaTecnicaListItem[]> {
  return request<VisitaTecnicaListItem[]>(`api/visitas-tecnicas?solicitudId=${solicitudId}`);
}
