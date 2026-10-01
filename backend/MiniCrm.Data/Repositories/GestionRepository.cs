using Microsoft.EntityFrameworkCore;
using MiniCrm.Domain.Entities;
using MiniCrm.Domain.Interfaces;

namespace MiniCrm.Data.Repositories;

public class GestionRepository : IGestionRepository
{
    private readonly MiniCrmContext _context;

    public GestionRepository(MiniCrmContext context)
    {
        _context = context;
    }

    public async Task<List<Gestion>> ListarAsync(int clienteId)
    {
        return await _context.Gestiones.AsNoTracking()
            .Where(gestion => gestion.ClienteId == clienteId)
            .OrderByDescending(gestion => gestion.FechaGestion)
            .ThenByDescending(gestion => gestion.Id)
            .ToListAsync();
    }

    public async Task AgregarAsync(Gestion gestion)
    {
        await _context.Gestiones.AddAsync(gestion);
    }

    public async Task GuardarAsync()
    {
        await _context.SaveChangesAsync();
    }
}
