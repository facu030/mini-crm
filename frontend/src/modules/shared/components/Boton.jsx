const variantes = {
  primario: 'bg-slate-900 text-white hover:bg-slate-700',
  secundario: 'border border-slate-300 bg-white text-slate-700 hover:bg-slate-50',
};

export default function Boton({ children, variante = 'primario', type = 'button', ...props }) {
  return (
    <button
      type={type}
      className={`rounded-lg px-4 py-2 text-sm font-medium transition-colors focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-slate-600 disabled:cursor-not-allowed disabled:opacity-50 ${variantes[variante]}`}
      {...props}
    >
      {children}
    </button>
  );
}
