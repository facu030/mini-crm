import { TIPOS_CONTACTO } from '../constants/gestiones';
import EstadoCliente from '../../clientes/components/EstadoCliente';
import Mensaje from '../../shared/components/Mensaje';
import { formatearFecha, formatearFechaHora } from '../../shared/helpers/fechas';

const encabezados = ['Fecha y hora', 'Tipo de contacto', 'Comentario', 'Estado resultante', 'Próximo contacto'];

export default function HistorialGestiones({ gestiones }) {
  return (
    <section aria-label="Historial de gestiones" className="space-y-3">
      <div>
        <h3 className="text-lg font-semibold text-slate-900">Historial de gestiones ({gestiones.length})</h3>
        <p className="mt-1 text-sm text-slate-500">De la más reciente a la más antigua.</p>
      </div>
      {gestiones.length === 0 ? (
        <Mensaje>Este cliente todavía no tiene gestiones registradas.</Mensaje>
      ) : (
        <div className="overflow-x-auto rounded-xl border border-slate-200 bg-white">
          <table className="w-full min-w-[800px] text-left text-sm">
            <caption className="sr-only">Historial completo de gestiones del cliente</caption>
            <thead className="border-b border-slate-200 bg-slate-100 text-xs text-slate-600">
              <tr>
                {encabezados.map((encabezado) => (
                  <th key={encabezado} scope="col" className="px-4 py-4 font-semibold">{encabezado}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {gestiones.map((gestion) => (
                <tr key={gestion.id}>
                  <td className="whitespace-nowrap px-4 py-5 align-top">{formatearFechaHora(gestion.fechaGestion)}</td>
                  <td className="px-4 py-5 align-top">{TIPOS_CONTACTO.find((tipo) => tipo.valor === gestion.tipoContacto)?.nombre || '—'}</td>
                  <td className="min-w-64 max-w-lg whitespace-pre-wrap break-words px-4 py-5 align-top">{gestion.comentario}</td>
                  <td className="px-4 py-5 align-top"><EstadoCliente valor={gestion.estadoResultante} /></td>
                  <td className="whitespace-nowrap px-4 py-5 align-top">{formatearFecha(gestion.proximoContacto)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
