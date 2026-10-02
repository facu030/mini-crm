import EstadoCliente from './EstadoCliente';
import Boton from '../../shared/components/Boton';
import { formatearFecha, formatearFechaHora } from '../../shared/helpers/fechas';

const encabezados = ['Nombre o razón social', 'CUIT', 'Teléfono', 'Correo electrónico', 'Estado', 'Asesor', 'Próximo contacto', 'Última actualización', 'Acciones'];

export default function ClientesTable({ clientes, onEditar, onVerDetalle }) {
  return (
    <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
      <table className="w-full min-w-[1100px] text-left text-sm">
        <caption className="sr-only">Clientes registrados y sus próximos contactos</caption>
        <thead className="border-b border-slate-200 bg-slate-100 text-xs text-slate-600">
          <tr>
            {encabezados.map((encabezado) => (
              <th key={encabezado} scope="col" className="px-4 py-4 font-semibold">{encabezado}</th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-slate-100">
          {clientes.map((cliente) => (
            <tr key={cliente.id} className={cliente.seguimientoVencido ? 'bg-amber-50/60' : ''}>
              <td className="max-w-52 break-words px-4 py-5 font-medium text-slate-900">{cliente.nombre}</td>
              <td className="whitespace-nowrap px-4 py-5">{cliente.cuit}</td>
              <td className="whitespace-nowrap px-4 py-5">{cliente.telefono || '—'}</td>
              <td className="max-w-52 break-all px-4 py-5">{cliente.email || '—'}</td>
              <td className="px-4 py-5"><EstadoCliente valor={cliente.estado} /></td>
              <td className="px-4 py-5">{cliente.asesor || '—'}</td>
              <td className="whitespace-nowrap px-4 py-5">
                <span>{formatearFecha(cliente.proximoContacto)}</span>
                {cliente.seguimientoVencido && (
                  <span className="mt-1 block text-xs font-semibold text-amber-800">Vencido</span>
                )}
              </td>
              <td className="whitespace-nowrap px-4 py-5 text-slate-500">
                {formatearFechaHora(cliente.fechaActualizacion)}
              </td>
              <td className="px-4 py-5">
                <div className="flex flex-wrap gap-2">
                  <Boton variante="secundario" onClick={() => onVerDetalle(cliente.id)} aria-label={`Ver detalle de ${cliente.nombre}`}>
                    Ver detalle
                  </Boton>
                  <Boton variante="secundario" onClick={() => onEditar(cliente)} aria-label={`Editar ${cliente.nombre}`}>
                    Editar
                  </Boton>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
