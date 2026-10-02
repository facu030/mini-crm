const indicadores = [
  { campo: 'totalClientes', titulo: 'Total de clientes' },
  { campo: 'prospectos', titulo: 'Prospectos' },
  { campo: 'interesados', titulo: 'Interesados' },
  { campo: 'seguimientosVencidos', titulo: 'Seguimientos vencidos' },
];

export default function ResumenGeneral({ resumen, cargando }) {
  return (
    <section aria-label="Resumen general" aria-busy={cargando} className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      {indicadores.map((indicador) => (
        <div key={indicador.campo} className="rounded-xl border border-slate-200 bg-white p-5">
          <p className="text-sm text-slate-500">{indicador.titulo}</p>
          <p className={`mt-2 text-3xl font-semibold ${indicador.campo === 'seguimientosVencidos' ? 'text-amber-700' : 'text-slate-900'}`}>
            {cargando ? '…' : (resumen?.[indicador.campo] ?? '—')}
          </p>
        </div>
      ))}
    </section>
  );
}
