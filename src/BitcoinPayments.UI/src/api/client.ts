import axios from 'axios';
import { API_BASE } from '../lib/constants';

const client = axios.create({ baseURL: API_BASE });

client.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

client.interceptors.response.use(
  (res) => res,
  async (error) => {
    if (error.response?.status === 401) {
      const refreshToken = localStorage.getItem('refresh_token');
      if (refreshToken && !error.config._retry) {
        error.config._retry = true;
        try {
          const res = await axios.post(`${API_BASE}/accounts/refresh-token`, {
            refreshToken,
          });
          const { accessToken, refreshToken: newRefresh } = res.data;
          localStorage.setItem('access_token', accessToken);
          localStorage.setItem('refresh_token', newRefresh);
          error.config.headers.Authorization = `Bearer ${accessToken}`;
          return client(error.config);
        } catch {
          localStorage.clear();
          window.location.href = '/login';
        }
      }
    }
    return Promise.reject(error);
  }
);

export default client;
