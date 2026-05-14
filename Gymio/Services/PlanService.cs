using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Gymio.Services
{
    public class PlanService : IPlanService
    {

        private readonly IDbContextFactory<GymioDbContext> _contextFactory;

        public PlanService(IDbContextFactory<GymioDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<bool> ActualizarPlanAsync(Plan plan)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();

            var planTrackeado = await _context.Planes.FindAsync(plan.Id);

            if (planTrackeado==null)
            {
                return false;
            }
            _context.Entry(planTrackeado).CurrentValues.SetValues(plan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CrearPlanAsync(Plan nuevoPlan)
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            _context.Planes.Add(nuevoPlan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Plan>> ObtenerPlanesAsync()
        {
            using var _context = await _contextFactory.CreateDbContextAsync();
            var planes = await  _context.Planes.OrderByDescending(p=>p.Precio).ToListAsync();
            return planes;
        }
    }
}
