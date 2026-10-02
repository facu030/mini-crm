import { useEffect, useState } from 'react';
import ClientesTable from '../components/ClientesTable';
import FiltrosClientes from '../components/FiltrosClientes';
import ClienteForm from '../components/ClienteForm';
import ClienteDetallePage from './ClienteDetallePage';
import { FILTROS_INICIALES } from '../constants/clientes';
import { listarClientes } from '../services/clientesService';
import ResumenGeneral from '../../dashboard/components/ResumenGeneral';
import { obtenerResumen } from '../../dashboard/services/dashboardService';
import Boton from '../../shared/components/Boton';
import Mensaje from '../../shared/components/Mensaje';
import { obtenerMensajeError } from '../../shared/helpers/errores';

export default function ClientesPage() {
  const [datos, setDatos] = useState({ clientes: [], resumen: null });
  const [filtros, setFiltros] = useState(FILTROS_INICIALES);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState('');
  const [formulario, setFormulario] = useState(null);
  const [clienteId, setClienteId] = useState(null);

  useEffect(() => {
    const controller = new AbortController();

    async function cargarDatos() {
      try {
        const [clientes, resumen] = await Promise.all([
          listarClientes(filtros, controller.signal),
          obtenerResumen(controller.signal),
        ]);

        if (!controller.signal.aborted) setDatos({ clientes, resumen });
      } catch (error) {
        if (!controller.signal.aborted) setError(obtenerMensajeError(error));
      } finally {
        if (!controller.signal.aborted) setCargando(false);
      }
    }

    cargarDatos();
    return () => controller.abort();
  }, [filtros]);

  function buscar(nuevosFiltros) {
    setCargando(true);
    setError('');
    setFiltros({ ...nuevosFiltros });
  }

  function actualizar() {
    buscar(filtros);
  }

  function clienteGuardado() {
    setFormulario(null);
    actualizar();
  }

  function volverDelDetalle() {
    setClienteId(null);
    actualizar();
  }

  if (formulario) {
    return (
      <ClienteForm
        cliente={formulario.cliente}
        onCancelar={() => setFormulario(null)}
        onGuardado={clienteGuardado}
      />
    );
  }

  if (clienteId !== null) {
    return <ClienteDetallePage clienteId={clienteId} onVolver={volverDelDetalle} />;
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-semibold text-slate-900">Clientes</h2>
          <p className="mt-1 text-sm text-slate-500">Consultá tu cartera y los próximos contactos.</p>
        </div>
        <div className="flex gap-2">
          <Boton variante="secundario" onClick={actualizar} disabled={cargando}>Actualizar</Boton>
          <Boton onClick={() => setFormulario({ cliente: null })}>Nuevo cliente</Boton>
        </div>
      </div>

      <ResumenGeneral resumen={error ? null : datos.resumen} cargando={cargando} />
      <FiltrosClientes onBuscar={buscar} filtrosIniciales={filtros} />

      <section aria-label="Listado de clientes" aria-busy={cargando} className="space-y-3">
        {cargando ? (
          <Mensaje>Cargando clientes y resumen…</Mensaje>
        ) : error ? (
          <Mensaje tipo="error">
            <p>{error}</p>
            <div className="mt-3"><Boton variante="secundario" onClick={actualizar}>Reintentar</Boton></div>
          </Mensaje>
        ) : datos.clientes.length === 0 ? (
          <Mensaje>
            {filtros.busqueda || filtros.estado
              ? 'No se encontraron clientes con esos filtros. Probá otra búsqueda o presioná Limpiar.'
              : 'Todavía no hay clientes registrados.'}
          </Mensaje>
        ) : (
          <>
            <p className="text-sm text-slate-500">Resultados: {datos.clientes.length}</p>
            <ClientesTable
              clientes={datos.clientes}
              onEditar={(cliente) => setFormulario({ cliente })}
              onVerDetalle={setClienteId}
            />
          </>
        )}
      </section>
    </div>
  );
}
