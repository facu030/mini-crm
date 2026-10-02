import { useState } from 'react';
import Swal from 'sweetalert2';
import { TIPOS_CONTACTO } from '../constants/gestiones';
import { crearGestion } from '../services/gestionesService';
import { ESTADOS_CLIENTE } from '../../clientes/constants/clientes';
import Boton from '../../shared/components/Boton';
import CampoFormulario from '../../shared/components/CampoFormulario';
import Mensaje from '../../shared/components/Mensaje';
import { obtenerMensajeError } from '../../shared/helpers/errores';

export default function GestionForm({ cliente, guardando, setGuardando, onGuardado }) {
  const [datos, setDatos] = useState({
    tipoContacto: 1,
    comentario: '',
    estadoResultante: cliente.estado,
    proximoContacto: '',
  });
  const [errores, setErrores] = useState({});
  const [error, setError] = useState('');

  function validarCampo(name, value, control) {
    if (name === 'tipoContacto' && !TIPOS_CONTACTO.some((tipo) => tipo.valor === Number(value))) {
      return 'El tipo de contacto no es válido.';
    }
    if (name === 'comentario' && !value.trim()) return 'El comentario es obligatorio.';
    if (name === 'estadoResultante' && !ESTADOS_CLIENTE.some((estado) => estado.valor === Number(value))) {
      return 'El estado resultante no es válido.';
    }
    if (name === 'proximoContacto' && control.validity.badInput) return 'La fecha no es válida.';
  }

  function cambiarCampo(event) {
    const { name, value } = event.target;
    const esNumero = name === 'tipoContacto' || name === 'estadoResultante';
    setDatos({ ...datos, [name]: esNumero ? Number(value) : value });
    const mensaje = errores[name] ? validarCampo(name, value, event.target) : undefined;
    setErrores((actuales) => {
      const nuevos = { ...actuales };
      if (mensaje) nuevos[name] = [mensaje];
      else delete nuevos[name];
      return nuevos;
    });
    setError('');
  }

  async function guardar(event) {
    event.preventDefault();
    if (guardando) return;

    const erroresLocales = {};
    Object.keys(datos).forEach((name) => {
      const mensaje = validarCampo(name, datos[name], event.currentTarget.elements[name]);
      if (mensaje) erroresLocales[name] = [mensaje];
    });
    setErrores(erroresLocales);
    setError('');
    if (Object.keys(erroresLocales).length > 0) return;

    setGuardando(true);
    try {
      const confirmacion = await Swal.fire({
        title: '¿Registrar la gestión?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: 'Registrar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#0f172a',
      });
      if (!confirmacion.isConfirmed) return;

      await crearGestion(cliente.id, {
        ...datos,
        comentario: datos.comentario.trim(),
        proximoContacto: datos.proximoContacto || null,
      });
      await Swal.fire({
        title: 'Gestión registrada',
        icon: 'success',
        confirmButtonText: 'Aceptar',
        confirmButtonColor: '#0f172a',
      });
      onGuardado();
    } catch (error) {
      const erroresCampos = error.response?.data?.errors ?? {};
      setErrores((actuales) => ({ ...actuales, ...erroresCampos }));
      setError(Object.keys(erroresCampos).length > 0 ? ''
        : obtenerMensajeError(error, 'No se pudo registrar la gestión. Intentá nuevamente.'));
    } finally {
      setGuardando(false);
    }
  }

  return (
    <section className="rounded-xl border border-slate-200 bg-white p-6">
      <h3 className="text-lg font-semibold text-slate-900">Registrar gestión</h3>
      <p className="mt-1 text-sm text-slate-500">Los campos con * son obligatorios. La fecha y hora se registran al guardar.</p>
      <form onSubmit={guardar} noValidate aria-label="Nueva gestión" className="mt-5 space-y-5">
        {error && <Mensaje tipo="error">{error}</Mensaje>}
        <fieldset disabled={guardando} className="grid gap-5 sm:grid-cols-2">
          <CampoFormulario
            label="Tipo de contacto" name="tipoContacto" required
            value={datos.tipoContacto} onChange={cambiarCampo} error={errores.tipoContacto?.[0]}
          >
            {TIPOS_CONTACTO.map((tipo) => (
              <option key={tipo.valor} value={tipo.valor}>{tipo.nombre}</option>
            ))}
          </CampoFormulario>
          <CampoFormulario
            label="Estado resultante" name="estadoResultante" required
            value={datos.estadoResultante} onChange={cambiarCampo} error={errores.estadoResultante?.[0]}
          >
            {ESTADOS_CLIENTE.map((estado) => (
              <option key={estado.valor} value={estado.valor}>{estado.nombre}</option>
            ))}
          </CampoFormulario>
          <div className="sm:col-span-2">
            <CampoFormulario
              label="Comentario" name="comentario" required multilinea rows={3}
              value={datos.comentario} onChange={cambiarCampo} error={errores.comentario?.[0]}
            />
          </div>
          <div>
            <CampoFormulario
              label="Próximo contacto" name="proximoContacto" type="date"
              value={datos.proximoContacto} onChange={cambiarCampo} error={errores.proximoContacto?.[0]}
            />
            <p className="mt-2 text-xs text-slate-500">Si no indicás una nueva fecha, se mantiene el próximo contacto actual.</p>
          </div>
        </fieldset>
        <div className="border-t border-slate-100 pt-5">
          <Boton type="submit" disabled={guardando}>{guardando ? 'Procesando…' : 'Registrar gestión'}</Boton>
        </div>
      </form>
    </section>
  );
}
