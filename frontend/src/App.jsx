import ClientesPage from './modules/clientes/pages/ClientesPage';

export default function App() {
  return (
    <div className="min-h-screen bg-slate-50 text-slate-800">
      <header className="border-b border-slate-200 bg-white">
        <div className="mx-auto flex max-w-7xl items-center gap-3 px-4 py-5 sm:px-6">
          <span className="rounded-lg bg-slate-900 px-3 py-2 text-sm font-bold text-white">
            MC
          </span>
          <div>
            <h1 className="text-xl font-semibold">Mini CRM</h1>
            <p className="text-sm text-slate-500">Seguimiento comercial</p>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6">
        <ClientesPage />
      </main>
    </div>
  );
}
