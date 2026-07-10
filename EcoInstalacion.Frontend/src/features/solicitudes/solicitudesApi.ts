import { request } from '../../services/httpClient';
import type { SolicitudListItem } from './types';

export function getSolicitudes(): Promise<SolicitudListItem[]> {
  return request<SolicitudListItem[]>('api/solicitudes');
}
