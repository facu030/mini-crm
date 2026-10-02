import { api } from '../../shared/api/axiosInstance';

export async function listarClientes(filtros, signal) {
  const { data } = await api.get('/clientes', { params: filtros, signal });
  return data;
}

export async function obtenerCliente(id, signal) {
  const { data } = await api.get(`/clientes/${id}`, { signal });
  return data;
}

export async function crearCliente(cliente) {
  const { data } = await api.post('/clientes', cliente);
  return data;
}

export async function editarCliente(id, cliente) {
  const { data } = await api.put(`/clientes/${id}`, cliente);
  return data;
}
