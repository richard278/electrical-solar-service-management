export interface ClienteListItem {
  id: number;
  nombreCompleto: string;
  dui: string;
  telefono?: string;
  correo?: string;
  direccion?: string;
  esActivo: boolean;
}

export interface ClienteDetail {
  id: number;
  nombreCompleto: string;
  dui: string;
  telefono?: string | null;
  direccion?: string | null;
  correo?: string | null;
  esActivo: boolean;
}

export interface ClienteCreatePayload {
  nombreCompleto: string;
  dui: string;
  telefono?: string;
  direccion?: string;
  correo?: string;
  // esActivo no incluido: el backend fuerza true al crear
}

export interface ClienteUpdatePayload {
  nombreCompleto: string;
  dui: string;
  telefono?: string;
  direccion?: string;
  correo?: string;
  esActivo: boolean; // obligatorio: controla activación/desactivación
}

export type ClienteEstadoFiltro = 'activos' | 'inactivos' | 'todos';

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ClientePagedQuery {
  q?: string;
  estado?: ClienteEstadoFiltro;
  pageNumber?: number;
  pageSize?: number;
}

