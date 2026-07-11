export type AppSection =
  | "overview"
  | "clientes"
  | "tecnicos"
  | "solicitudes"
  | "visitas";

export interface NavigationItem {
  id: AppSection;
  label: string;
  description: string;
}

export const navigationItems: NavigationItem[] = [
  {
    id: "overview",
    label: "Inicio",
    description: "Resumen del sistema y métricas clave",
  },
  {
    id: "clientes",
    label: "Clientes",
    description: "Catálogo de clientes registrados",
  },
  {
    id: "tecnicos",
    label: "Técnicos",
    description: "Catálogo de personal técnico",
  },
  {
    id: "solicitudes",
    label: "Solicitudes",
    description: "Solicitudes de instalación eléctrica/solar",
  },
  {
    id: "visitas",
    label: "Visitas Técnicas",
    description: "Visitas de campo y aprobaciones",
  },
];
