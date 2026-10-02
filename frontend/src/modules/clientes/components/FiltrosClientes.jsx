import { useState } from 'react';
import Boton from '../../shared/components/Boton';
import { ESTADOS_CLIENTE, FILTROS_INICIALES } from '../constants/clientes';

const estiloCampo = 'mt-2 w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm focus:border-slate-500 focus:outline-2 focus:outline-slate-200';

export default function FiltrosClientes({ onBuscar, filtrosIniciales = FILTROS_INICIALES }) {
  const [filtros, setFiltros] = useState(filtrosIniciales);

  function cambiarFiltro(event) {
    const { name, value } = event.target;
    setFiltros({ ...filtros, [name]: value });
  }

  function buscar(event) {
    event.preventDefault();
    onBuscar({ ...filtros, busqueda: filtros.busqueda.trim() });
  }

  function limpiar() {
    setFiltros(FILTROS_INICIALES);
    onBuscar(FILTROS_INICIALES);
  }

  return (
    <form
      onSubmit={buscar}
      aria-label="Filtros de clientes"
      className="grid items-end gap-4 rounded-xl border border-slate-200 bg-white p-5 md:grid-cols-2 xl:grid-cols-[minmax(260px,1fr)_180px_210px_auto]"
    >
      <label className="text-sm font-medium">
        Buscar cliente
        <input
          name="busqueda"
          type="search"
          value={filtros.busqueda}
          onChange={cambiarFiltro}
          placeholder="Nombre, CUIT o teléfono"
          className={estiloCampo}
        />
      </label>

      <label className="text-sm font-medium">
        Estado
        <select name="estado" value={filtros.estado} onChange={cambiarFiltro} className={estiloCampo}>
          <option value="">Todos los estados</option>
          {ESTADOS_CLIENTE.map((estado) => (
            <option key={estado.valor} value={estado.valor}>{estado.nombre}</option>
          ))}
        </select>
      </label>

      <label className="text-sm font-medium">
        Próximo contacto
        <select name="orden" value={filtros.orden} onChange={cambiarFiltro} className={estiloCampo}>
          <option value="asc">Más cercano primero</option>
          <option value="desc">Más lejano primero</option>
        </select>
      </label>

      <div className="flex gap-2">
        <Boton type="submit">Buscar</Boton>
        <Boton variante="secundario" onClick={limpiar}>Limpiar</Boton>
      </div>
    </form>
  );
}
