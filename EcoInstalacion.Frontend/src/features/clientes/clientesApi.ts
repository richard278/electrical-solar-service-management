import { request } from '../../services/httpClient';
import type {
  ClienteListItem,
  ClienteDetail,
  ClienteCreatePayload,
  ClienteUpdatePayload,
  ClienteEstadoFiltro
} from './types';

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
