export default function Mensaje({ children, tipo = 'informacion' }) {
  const colores = tipo === 'error'
    ? 'border-red-200 bg-red-50 text-red-800'
    : 'border-slate-200 bg-white text-slate-600';

  return (
    <div
      role={tipo === 'error' ? 'alert' : 'status'}
      className={`rounded-lg border p-5 text-sm ${colores}`}
    >
      {children}
    </div>
  );
}
