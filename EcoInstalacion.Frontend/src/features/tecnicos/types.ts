import type { PagedResponse } from '../../types/pagination';

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

export type PagedResult<T> = PagedResponse<T>;

export interface TecnicoPagedQuery {
  q?: string;
  page?: number;
  pageSize?: number;
}
