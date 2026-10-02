export default function CampoFormulario({ label, name, error, required = false, multilinea = false, children, ...props }) {
  const atributos = {
    id: name,
    name,
    required,
    'aria-invalid': Boolean(error),
    'aria-describedby': error ? `${name}-error` : undefined,
    className: `mt-2 w-full rounded-lg border bg-white px-3 py-2 text-sm focus:outline-2 focus:outline-slate-200 ${error ? 'border-red-400' : 'border-slate-300'}`,
    ...props,
  };

  return (
    <div>
      <label htmlFor={name} className="text-sm font-medium">{label}{required ? ' *' : ''}</label>
      {children ? <select {...atributos}>{children}</select>
        : multilinea ? <textarea {...atributos} /> : <input {...atributos} />}
      {error && <p id={`${name}-error`} className="mt-1 text-sm text-red-700">{error}</p>}
    </div>
  );
}
