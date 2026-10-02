import { api } from '../../shared/api/axiosInstance';

export async function obtenerResumen(signal) {
  const { data } = await api.get('/dashboard/resumen', { signal });
  return data;
}
