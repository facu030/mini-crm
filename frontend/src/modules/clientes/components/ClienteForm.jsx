import { useState } from 'react';
import Swal from 'sweetalert2';
import { ESTADOS_CLIENTE } from '../constants/clientes';
import { crearCliente, editarCliente } from '../services/clientesService';
import Boton from '../../shared/components/Boton';
import CampoFormulario from '../../shared/components/CampoFormulario';
import Mensaje from '../../shared/components/Mensaje';
import { obtenerMensajeError } from '../../shared/helpers/errores';

export default function ClienteForm({ cliente, onCancelar, onGuardado }) {
  const [datos, setDatos] = useState({
    nombre: cliente?.nombre ?? '',
    cuit: cliente?.cuit ?? '',
    telefono: cliente?.telefono ?? '',
    email: cliente?.email ?? '',
    estado: cliente?.estado ?? 1,
    asesor: cliente?.asesor ?? '',
  });
  const [errores, setErrores] = useState({});
  const [error, setError] = useState('');
  const [guardando, setGuardando] = useState(false);

  function validarCampo(name, value, control) {
    if (name === 'nombre' && !value.trim()) return 'El nombre es obligatorio.';
    if (name === 'cuit') {
      const cuit = value.replace(/[\s-]/g, '');
      if (!cuit) return 'El CUIT es obligatorio.';
      if (!/^[0-9]{11}$/.test(cuit)) return 'El CUIT debe tener 11 dígitos numéricos.';
      // Conserva el error del servidor mientras siga siendo el mismo CUIT.
      if (errores.cuit && cuit === datos.cuit.replace(/[\s-]/g, '')) return errores.cuit[0];
    }
    if (name === 'email' && control.validity.typeMismatch) {
      return 'El correo electrónico no tiene un formato válido.';
    }
    if (name === 'estado' && !ESTADOS_CLIENTE.some((estado) => estado.valor === Number(value))) {
      return 'El estado no es válido.';
    }
  }

  function cambiarCampo(event) {
    const { name, value } = event.target;
    setDatos({ ...datos, [name]: name === 'estado' ? Number(value) : value });
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
    ['nombre', 'cuit', 'email', 'estado'].forEach((name) => {
      const mensaje = validarCampo(name, datos[name], event.currentTarget.elements[name]);
      if (mensaje) erroresLocales[name] = [mensaje];
    });

    setErrores(erroresLocales);
    setError('');
    if (Object.keys(erroresLocales).length > 0) return;

    setGuardando(true);
    try {
      const confirmacion = await Swal.fire({
        title: cliente ? '¿Guardar los cambios?' : '¿Crear el cliente?',
        icon: 'question',
        showCancelButton: true,
        confirmButtonText: cliente ? 'Guardar' : 'Crear',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#0f172a',
      });

      if (!confirmacion.isConfirmed) return;

      if (cliente) {
        await editarCliente(cliente.id, datos);
      } else {
        await crearCliente(datos);
      }

      await Swal.fire({
        title: cliente ? 'Cliente actualizado' : 'Cliente creado',
        icon: 'success',
        confirmButtonText: 'Aceptar',
        confirmButtonColor: '#0f172a',
      });
      onGuardado();
    } catch (error) {
      const mensaje = obtenerMensajeError(error, 'No se pudo guardar el cliente. Intentá nuevamente.');
      const erroresCampos = error.response?.status === 409
        ? { cuit: [mensaje] }
        : (error.response?.data?.errors ?? {});
      setErrores((actuales) => ({ ...actuales, ...erroresCampos }));
      setError(Object.keys(erroresCampos).length > 0 ? '' : mensaje);
    } finally {
      setGuardando(false);
    }
  }

  return (
    <div className="mx-auto max-w-3xl space-y-5">
      <div>
        <h2 className="text-2xl font-semibold text-slate-900">{cliente ? 'Editar cliente' : 'Nuevo cliente'}</h2>
        <p className="mt-1 text-sm text-slate-500">Los campos con * son obligatorios.</p>
      </div>

      {error && <Mensaje tipo="error">{error}</Mensaje>}

      <form onSubmit={guardar} noValidate aria-label="Datos del cliente" className="rounded-xl border border-slate-200 bg-white p-6">
        <fieldset disabled={guardando} className="grid gap-5 sm:grid-cols-2">
          <CampoFormulario
            label="Nombre o razón social" name="nombre" required
            value={datos.nombre} onChange={cambiarCampo} error={errores.nombre?.[0]}
            autoComplete="organization"
          />
          <CampoFormulario
            label="CUIT" name="cuit" required
            value={datos.cuit} onChange={cambiarCampo} error={errores.cuit?.[0]}
            inputMode="numeric" placeholder="Ej.: 30-12345678-9"
          />
          <CampoFormulario
            label="Teléfono" name="telefono" type="tel"
            value={datos.telefono} onChange={cambiarCampo} error={errores.telefono?.[0]}
            autoComplete="tel"
          />
          <CampoFormulario
            label="Correo electrónico" name="email" type="email"
            value={datos.email} onChange={cambiarCampo} error={errores.email?.[0]}
            autoComplete="email"
          />
          <CampoFormulario
            label="Estado" name="estado" required
            value={datos.estado} onChange={cambiarCampo} error={errores.estado?.[0]}
          >
            {ESTADOS_CLIENTE.map((estado) => (
              <option key={estado.valor} value={estado.valor}>{estado.nombre}</option>
            ))}
          </CampoFormulario>
          <CampoFormulario
            label="Asesor responsable" name="asesor"
            value={datos.asesor} onChange={cambiarCampo} error={errores.asesor?.[0]}
          />
        </fieldset>

        <div className="mt-6 flex flex-wrap gap-3 border-t border-slate-100 pt-5">
          <Boton type="submit" disabled={guardando}>
            {guardando ? 'Procesando…' : (cliente ? 'Guardar cambios' : 'Crear cliente')}
          </Boton>
          <Boton variante="secundario" onClick={onCancelar} disabled={guardando}>Cancelar</Boton>
        </div>
      </form>
    </div>
  );
}
