import { api } from '../../shared/api/axiosInstance';

export async function listarGestiones(clienteId, signal) {
  const { data } = await api.get(`/clientes/${clienteId}/gestiones`, { signal });
  return data;
}

export async function crearGestion(clienteId, gestion) {
  const { data } = await api.post(`/clientes/${clienteId}/gestiones`, gestion);
  return data;
}
