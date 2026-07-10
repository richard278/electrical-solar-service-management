export type SolicitudListItem = {
  id: number;
  fechaSolicitud?: string;
  clienteId?: number;
  clienteNombre?: string;
  tipoInstalacion?: string;
  estado?: string;
  tecnicoAsignadoId?: number;
  tecnicoNombre?: string;
};
