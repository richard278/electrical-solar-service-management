export interface AuthUser {
  id: number;
  nombre: string;
  rol: string;
}

export interface AuthLoginRequest {
  correo: string;
  clave: string;
}

export interface AuthLoginResponse {
  accessToken: string;
  tokenType: string;
  expiresAtUtc: string;
  user: AuthUser;
}

export interface AuthSession {
  user: AuthUser;
  accessToken: string;
  expiresAtUtc: string;
}
