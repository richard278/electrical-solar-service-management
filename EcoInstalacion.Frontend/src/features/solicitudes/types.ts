export const SOLICITUD_ESTADOS = [
  'Pendiente',
  'Programada',
  'En proceso',
  'Cerrada',
  'Rechazada'
] as const;

export type SolicitudEstado = (typeof SOLICITUD_ESTADOS)[number];

export interface SolicitudListItem {
  id: number;
  fechaSolicitud: string;
  clienteId: number;
  clienteNombre?: string | null;
  tipoInstalacion?: string | null;
  estado?: string | null;
  tecnicoAsignadoId?: number | null;
  tecnicoNombre?: string | null;
}

export interface SolicitudDetail extends SolicitudListItem {
  descripcion?: string | null;
}

export interface SolicitudCreatePayload {
  clienteId: number;
  tipoInstalacion: string;
  descripcion?: string | null;
  tecnicoAsignadoId?: number | null;
}

export interface SolicitudUpdatePayload extends SolicitudCreatePayload {
  estado: SolicitudEstado;
}

export interface PagedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface SolicitudPagedQuery {
  q?: string;
  estado?: string;
  clienteId?: number;
  tecnicoAsignadoId?: number;
  tipoInstalacion?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  clienteNombre?: string;
  tecnicoNombre?: string;
  pageNumber?: number;
  pageSize?: number;
}

