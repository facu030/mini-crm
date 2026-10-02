import { ESTADOS_CLIENTE } from '../constants/clientes';

export default function EstadoCliente({ valor }) {
  const estado = ESTADOS_CLIENTE.find((opcion) => opcion.valor === valor);

  return (
    <span className={`inline-block whitespace-nowrap rounded-full px-2.5 py-1 text-xs font-medium ${estado?.color || 'bg-slate-100 text-slate-700'}`}>
      {estado?.nombre || 'Sin estado'}
    </span>
  );
}
