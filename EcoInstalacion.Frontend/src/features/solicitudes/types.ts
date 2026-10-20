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

export interface SolicitudPagedQuery {
  q?: string;
  estado?: string;
  page?: number;
  pageSize?: number;
}
