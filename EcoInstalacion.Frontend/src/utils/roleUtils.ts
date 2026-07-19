import type { AuthUser } from '../types/auth';

/**
 * Normalizes a role string by trimming whitespace, lowercasing,
 * and stripping diacritics/accents for robust role comparison.
 */
export function normalizeRole(role?: string | null): string {
  if (!role) return '';
  return role
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '');
}

/**
 * Checks if the authenticated user has permission to create a Cliente.
 * Allowed for: Administrador and Técnico.
 */
export function canCreateCliente(user?: AuthUser | null): boolean {
  const norm = normalizeRole(user?.rol);
  return norm === 'administrador' || norm === 'tecnico';
}

/**
 * Checks if the authenticated user has permission to edit a Cliente.
 * Allowed for: Administrador only.
 */
export function canEditCliente(user?: AuthUser | null): boolean {
  const norm = normalizeRole(user?.rol);
  return norm === 'administrador';
}

/**
 * Checks if the authenticated user has permission to delete a Cliente.
 * Allowed for: Administrador only.
 */
export function canDeleteCliente(user?: AuthUser | null): boolean {
  const norm = normalizeRole(user?.rol);
  return norm === 'administrador';
}

/** Administrador puede crear registros del catálogo de personal técnico. */
export function canCreateTecnico(user?: AuthUser | null): boolean {
  return normalizeRole(user?.rol) === 'administrador';
}

/** Administrador puede editar registros del catálogo de personal técnico. */
export function canEditTecnico(user?: AuthUser | null): boolean {
  return normalizeRole(user?.rol) === 'administrador';
}

/** Administrador puede eliminar técnicos que no tengan solicitudes asociadas. */
export function canDeleteTecnico(user?: AuthUser | null): boolean {
  return normalizeRole(user?.rol) === 'administrador';
}
