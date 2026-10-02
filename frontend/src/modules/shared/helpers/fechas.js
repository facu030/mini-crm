export function formatearFecha(fecha) {
  if (!fecha) return 'Sin fecha';

  // DateOnly llega como AAAA-MM-DD. No se convierte a UTC para evitar cambiar el día.
  const [anio, mes, dia] = fecha.split('-');
  return `${dia}/${mes}/${anio}`;
}

export function formatearFechaHora(fecha) {
  if (!fecha) return 'Sin fecha';

  return new Intl.DateTimeFormat('es-AR', {
    dateStyle: 'short',
    timeStyle: 'short',
  }).format(new Date(fecha));
}
