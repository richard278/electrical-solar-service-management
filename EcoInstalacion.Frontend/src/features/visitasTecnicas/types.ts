export interface VisitaTecnicaListItem {
  id: number;
  solicitudId: number;
  fechaVisita: string;
  observaciones?: string | null;
  aprobado: boolean;
  clienteNombre?: string | null;
  tipoInstalacion?: string | null;
  estadoSolicitud?: string | null;
  tecnicoNombre?: string | null;
}

export type VisitaTecnicaDetail = VisitaTecnicaListItem;

export interface VisitaTecnicaCreatePayload {
  solicitudId: number;
  fechaVisita: string;
  observaciones?: string | null;
  aprobado: boolean;
}

export interface VisitaTecnicaUpdatePayload {
  fechaVisita: string;
  observaciones?: string | null;
  aprobado: boolean;
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

export interface VisitaTecnicaPagedQuery {
  q?: string;
  aprobado?: boolean;
  solicitudId?: number;
  estadoSolicitud?: string;
  clienteNombre?: string;
  tecnicoNombre?: string;
  tipoInstalacion?: string;
  fechaDesde?: string;
  fechaHasta?: string;
  pageNumber?: number;
  pageSize?: number;
}

