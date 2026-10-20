import { request } from '../../services/httpClient';
import type { PagedResponse } from '../../types/pagination';
import type {
  ClienteListItem,
  ClienteDetail,
  ClienteCreatePayload,
  ClienteUpdatePayload,
  ClienteEstadoFiltro,
  ClientePagedQuery
} from './types';

function buildClientesQueryParams(query: ClientePagedQuery = {}): string {
  const params = new URLSearchParams();
  if (query.q && query.q.trim() !== '') {
    params.append('q', query.q.trim());
  }
  if (query.estado) {
    params.append('estado', query.estado);
  }
  if (query.page !== undefined && query.page !== null) {
    params.append('page', query.page.toString());
  }
  if (query.pageSize !== undefined && query.pageSize !== null) {
    params.append('pageSize', query.pageSize.toString());
  }
  return params.toString();
}

/** Listado de clientes paginados y buscados */
export function getClientesPaginados(
  query: ClientePagedQuery = {}
): Promise<PagedResponse<ClienteListItem>> {
  const queryString = buildClientesQueryParams(query);
  const url = `api/clientes/paginados${queryString ? `?${queryString}` : ''}`;

  return request<PagedResponse<ClienteListItem>>(url);
}

/** Listado de clientes activos — para Administrador y Técnico (selectores / resumen) */
export function getClientes(): Promise<ClienteListItem[]> {
  return request<ClienteListItem[]>('api/clientes');
}

/**
 * Listado administrativo paginado con filtro de estado y búsqueda.
 * Solo Administrador tiene acceso (ApiAdminOnly en backend).
 */
export function getClientesAdministracion(
  estadoOrQuery: ClienteEstadoFiltro | ClientePagedQuery = 'activos'
): Promise<PagedResponse<ClienteListItem>> {
  const query: ClientePagedQuery =
    typeof estadoOrQuery === 'string'
      ? { estado: estadoOrQuery }
      : { estado: 'activos', ...estadoOrQuery };

  const queryString = buildClientesQueryParams(query);
  const url = `api/clientes/administracion${queryString ? `?${queryString}` : ''}`;

  return request<PagedResponse<ClienteListItem>>(url);
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
