import { request } from '../../services/httpClient';
import type { TecnicoListItem } from './types';

export function getTecnicos(): Promise<TecnicoListItem[]> {
  return request<TecnicoListItem[]>('api/tecnicos');
}
