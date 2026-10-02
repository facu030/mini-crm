import EstadoCliente from './EstadoCliente';
import { formatearFecha, formatearFechaHora } from '../../shared/helpers/fechas';

export default function ClienteInfo({ cliente }) {
  const campos = [
    { nombre: 'CUIT', valor: cliente.cuit },
    { nombre: 'Teléfono', valor: cliente.telefono || '—' },
    { nombre: 'Correo electrónico', valor: cliente.email || '—' },
    { nombre: 'Estado actual', valor: <EstadoCliente valor={cliente.estado} /> },
    { nombre: 'Asesor responsable', valor: cliente.asesor || '—' },
    {
      nombre: 'Próximo contacto',
      valor: <>{formatearFecha(cliente.proximoContacto)}{cliente.seguimientoVencido && (
        <span className="ml-2 text-xs font-semibold text-amber-800">Vencido</span>
      )}</>,
    },
    { nombre: 'Fecha de creación', valor: formatearFechaHora(cliente.fechaCreacion) },
    { nombre: 'Última actualización', valor: formatearFechaHora(cliente.fechaActualizacion) },
  ];

  return (
    <section aria-label="Datos del cliente" className="rounded-xl border border-slate-200 bg-white p-6">
      <h3 className="mb-5 text-lg font-semibold text-slate-900">Datos del cliente</h3>
      <dl className="grid gap-5 sm:grid-cols-2 lg:grid-cols-4">
        {campos.map((campo) => (
          <div key={campo.nombre}>
            <dt className="text-sm text-slate-500">{campo.nombre}</dt>
            <dd className="mt-1 break-words text-sm font-medium text-slate-900">{campo.valor}</dd>
          </div>
        ))}
      </dl>
    </section>
  );
}
