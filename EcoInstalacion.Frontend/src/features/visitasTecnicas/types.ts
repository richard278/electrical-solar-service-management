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

export interface VisitaTecnicaPagedQuery {
  q?: string;
  aprobado?: boolean;
  page?: number;
  pageSize?: number;
}
