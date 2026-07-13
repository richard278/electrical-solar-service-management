import { API_BASE_URL } from '../config/environment';

let tokenGetter: (() => string | null) | null = null;

export function setTokenGetter(getter: () => string | null) {
  tokenGetter = getter;
}

export class HttpError extends Error {
  status: number;
  constructor(status: number, message: string) {
    super(message);
    this.status = status;
    this.name = 'HttpError';
  }
}

export async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const url = `${API_BASE_URL.replace(/\/$/, '')}/${path.replace(/^\//, '')}`;
  
  const headers = new Headers(options.headers);
  if (!headers.has('Accept')) {
    headers.set('Accept', 'application/json');
  }

  if (tokenGetter) {
    const token = tokenGetter();
    if (token && !headers.has('Authorization')) {
      headers.set('Authorization', `Bearer ${token}`);
    }
  }

  const config: RequestInit = {
    ...options,
    headers,
  };

  try {
    const response = await fetch(url, config);

    if (!response.ok) {
      if (response.status === 401) {
        throw new HttpError(401, 'Sesión no autorizada o expirada.');
      }
      let errorMessage = `Error HTTP: ${response.status} ${response.statusText}`;
      try {
        const errorData = await response.json();
        if (errorData) {
          errorMessage = errorData.mensaje || errorData.message || errorMessage;
        }
      } catch {
        // Fallback si no es un JSON
      }
      throw new HttpError(response.status, errorMessage);
    }

    if (response.status === 204) {
      return null as unknown as T;
    }

    return await response.json() as T;
  } catch (error) {
    if (error instanceof HttpError || error instanceof Error) {
      throw error;
    }
    throw new Error('Ocurrió un error desconocido al procesar la solicitud.');
  }
}
