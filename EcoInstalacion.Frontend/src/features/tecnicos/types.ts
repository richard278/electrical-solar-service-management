export interface TecnicoListItem {
  id: number;
  nombre: string;
  especialidad?: string | null;
  telefono?: string | null;
  correo?: string | null;
}

export interface TecnicoDetail {
  id: number;
  nombre: string;
  especialidad?: string | null;
  telefono?: string | null;
  correo?: string | null;
}

export interface TecnicoCreatePayload {
  nombre: string;
  especialidad?: string;
  telefono?: string;
  correo?: string;
}

export interface TecnicoUpdatePayload {
  nombre: string;
  especialidad?: string;
  telefono?: string;
  correo?: string;
}

export type TecnicoEstadoFiltro = 'activos' | 'inactivos' | 'todos';

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface TecnicoPagedQuery {
  q?: string;
  estado?: TecnicoEstadoFiltro;
  pageNumber?: number;
  pageSize?: number;
}
