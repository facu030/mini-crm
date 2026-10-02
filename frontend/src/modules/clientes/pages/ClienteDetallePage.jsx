import { useEffect, useState } from 'react';
import ClienteInfo from '../components/ClienteInfo';
import { obtenerCliente } from '../services/clientesService';
import GestionForm from '../../gestiones/components/GestionForm';
import HistorialGestiones from '../../gestiones/components/HistorialGestiones';
import { listarGestiones } from '../../gestiones/services/gestionesService';
import Boton from '../../shared/components/Boton';
import Mensaje from '../../shared/components/Mensaje';
import { obtenerMensajeError } from '../../shared/helpers/errores';

export default function ClienteDetallePage({ clienteId, onVolver }) {
  const [datos, setDatos] = useState({ cliente: null, gestiones: [] });
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState('');
  const [guardando, setGuardando] = useState(false);
  const [recarga, setRecarga] = useState(0);

  useEffect(() => {
    const controller = new AbortController();

    async function cargarDatos() {
      try {
        const [cliente, gestiones] = await Promise.all([
          obtenerCliente(clienteId, controller.signal),
          listarGestiones(clienteId, controller.signal),
        ]);
        if (!controller.signal.aborted) setDatos({ cliente, gestiones });
      } catch (error) {
        if (!controller.signal.aborted) setError(obtenerMensajeError(error));
      } finally {
        if (!controller.signal.aborted) setCargando(false);
      }
    }

    cargarDatos();
    return () => controller.abort();
  }, [clienteId, recarga]);

  function actualizar() {
    setCargando(true);
    setError('');
    setRecarga((actual) => actual + 1);
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="text-2xl font-semibold text-slate-900">{datos.cliente?.nombre || 'Detalle del cliente'}</h2>
          <p className="mt-1 text-sm text-slate-500">Datos del cliente y seguimiento comercial.</p>
        </div>
        <Boton variante="secundario" onClick={onVolver} disabled={guardando}>Volver al listado</Boton>
      </div>

      <div aria-busy={cargando} className="space-y-6">
        {cargando ? (
          <Mensaje>Cargando cliente e historial…</Mensaje>
        ) : error ? (
          <Mensaje tipo="error">
            <p>{error}</p>
            <div className="mt-3"><Boton variante="secundario" onClick={actualizar}>Reintentar</Boton></div>
          </Mensaje>
        ) : (
          <>
            <ClienteInfo cliente={datos.cliente} />
            <GestionForm
              cliente={datos.cliente}
              guardando={guardando}
              setGuardando={setGuardando}
              onGuardado={actualizar}
            />
            <HistorialGestiones gestiones={datos.gestiones} />
          </>
        )}
      </div>
    </div>
  );
}
