export type VisitaTecnicaListItem = {
  id: number;
  solicitudId?: number;
  fechaVisita?: string;
  observaciones?: string;
  aprobado?: boolean | null;
};
