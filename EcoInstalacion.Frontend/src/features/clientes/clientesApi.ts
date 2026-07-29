import { request } from '../../services/httpClient';
import type {
  ClienteListItem,
  ClienteDetail,
  ClienteCreatePayload,
  ClienteUpdatePayload,
  ClienteEstadoFiltro,
  PagedResult,
  ClientePagedQuery
} from './types';

/** Listado de clientes paginados y buscados */
export function getClientesPaginados(
  query: ClientePagedQuery
): Promise<PagedResult<ClienteListItem>> {
  const params = new URLSearchParams();
  if (query.q && query.q.trim() !== '') {
    params.append('q', query.q.trim());
  }
  if (query.estado) {
    params.append('estado', query.estado);
  }
  if (query.pageNumber !== undefined && query.pageNumber !== null) {
    params.append('pageNumber', query.pageNumber.toString());
  }
  if (query.pageSize !== undefined && query.pageSize !== null) {
    params.append('pageSize', query.pageSize.toString());
  }

  const queryString = params.toString();
  const url = `api/clientes/paginados${queryString ? `?${queryString}` : ''}`;

  return request<PagedResult<ClienteListItem>>(url);
}

/** Listado de clientes activos — para Administrador y Técnico */
export function getClientes(): Promise<ClienteListItem[]> {
  return request<ClienteListItem[]>('api/clientes');
}

/**
 * Listado administrativo con filtro de estado.
 * Solo Administrador tiene acceso (ApiAdminOnly en backend).
 */
export function getClientesAdministracion(
  estado: ClienteEstadoFiltro
): Promise<ClienteListItem[]> {
  return request<ClienteListItem[]>(
    `api/clientes/administracion?estado=${encodeURIComponent(estado)}`
  );
}

export function getClienteById(id: number): Promise<ClienteDetail> {
  return request<ClienteDetail>(`api/clientes/${id}`);
}

export function createCliente(payload: ClienteCreatePayload): Promise<ClienteDetail> {
  return request<ClienteDetail>('api/clientes', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function updateCliente(
  id: number,
  payload: ClienteUpdatePayload
): Promise<ClienteDetail> {
  return request<ClienteDetail>(`api/clientes/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(payload)
  });
}

export function deleteCliente(id: number): Promise<void> {
  return request<void>(`api/clientes/${id}`, { method: 'DELETE' });
}
