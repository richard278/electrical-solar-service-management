import { API_BASE_URL } from '../config/environment';

let tokenGetter: (() => string | null) | null = null;

export function setTokenGetter(getter: () => string | null) {
  tokenGetter = getter;
}

export class HttpError extends Error {
  status: number;
  validationErrors?: Record<string, string[]>;

  constructor(
    status: number,
    message: string,
    validationErrors?: Record<string, string[]>
  ) {
    super(message);
    this.status = status;
    this.name = 'HttpError';
    if (validationErrors !== undefined) {
      this.validationErrors = validationErrors;
    }
  }
}

/** Returns true if value is a Record<string, string[]> */
function isValidationErrorsRecord(
  value: unknown
): value is Record<string, string[]> {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) {
    return false;
  }
  return Object.values(value as Record<string, unknown>).every(
    (v) => Array.isArray(v) && (v as unknown[]).every((s) => typeof s === 'string')
  );
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
      let parsedValidationErrors: Record<string, string[]> | undefined;

      try {
        const errorData: unknown = await response.json();
        if (errorData !== null && typeof errorData === 'object') {
          const obj = errorData as Record<string, unknown>;
          // Extract human-readable message (prefer 'mensaje' for Spanish APIs)
          const rawMessage = obj['mensaje'] ?? obj['message'];
          if (typeof rawMessage === 'string' && rawMessage.trim().length > 0) {
            errorMessage = rawMessage.trim();
          }
          // Extract ValidationProblemDetails.errors
          const rawErrors = obj['errors'];
          if (isValidationErrorsRecord(rawErrors)) {
            parsedValidationErrors = rawErrors;
          }
        }
      } catch {
        // Response body is not JSON — keep default errorMessage
      }

      throw new HttpError(response.status, errorMessage, parsedValidationErrors);
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
