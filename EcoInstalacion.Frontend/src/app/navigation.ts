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
    description: "Gestión de clientes registrados",
  },
  {
    id: "tecnicos",
    label: "Técnicos",
    description: "Gestión del personal técnico",
  },
  {
    id: "solicitudes",
    label: "Solicitudes",
    description: "Gestión de solicitudes eléctricas y solares",
  },
  {
    id: "visitas",
    label: "Visitas Técnicas",
    description: "Gestión de visitas y aprobaciones",
  },
];
