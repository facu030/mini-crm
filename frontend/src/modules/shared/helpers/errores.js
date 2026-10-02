export function obtenerMensajeError(error, mensaje = 'No se pudieron cargar los datos. Intentá nuevamente.') {
  if (!error.response) {
    return 'No se pudo conectar con el servidor. Intentá nuevamente en unos momentos.';
  }

  return error.response.data?.title || mensaje;
}
