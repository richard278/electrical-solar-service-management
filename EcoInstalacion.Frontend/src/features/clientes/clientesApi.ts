import { request } from '../../services/httpClient';
import type { ClienteListItem } from './types';

export function getClientes(): Promise<ClienteListItem[]> {
  return request<ClienteListItem[]>('api/clientes');
}
