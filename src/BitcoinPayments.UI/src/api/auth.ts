import client from './client';

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: { id: string; email: string; displayName: string | null };
}

export async function login(email: string, password: string): Promise<AuthResponse> {
  const { data } = await client.post<AuthResponse>('/accounts/login', { email, password });
  return data;
}

export async function register(email: string, password: string, displayName?: string): Promise<AuthResponse> {
  const { data } = await client.post<AuthResponse>('/accounts/register', { email, password, displayName });
  return data;
}

export async function demoLogin(): Promise<AuthResponse> {
  const { data } = await client.post<AuthResponse>('/accounts/demo-login');
  return data;
}
