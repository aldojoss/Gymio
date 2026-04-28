using Gymio.Data;
using Gymio.Interfaces;
using Gymio.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Gymio.Services
{
    public class PlanService : IPlanService
    {

        private readonly GymioDbContext _context;

        public PlanService (GymioDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ActualizarPlanAsync(Plan plan)
        {

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

           _context.Planes.Add(nuevoPlan);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Plan>> ObtenerPlanesAsync()
        {
            var planes = await  _context.Planes.OrderByDescending(p=>p.Precio).ToListAsync();
            return planes;
        }
    }
}
