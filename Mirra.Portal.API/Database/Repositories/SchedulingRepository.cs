using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories
{
    public class SchedulingRepository : DefaultRepository, ISchedulingRepository
    {
        public SchedulingRepository(DatabaseContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<List<Scheduling>> GetAllByConfigurationId(int configurationId)
        {
            return await _context.Schedulings
                .AsNoTracking()
                .Where(scheduling => scheduling.CustomerContentPlatformConfigurationId == configurationId)
                .ProjectTo<Scheduling>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<Scheduling> GetById(int schedulingId)
        {
            return await _context.Schedulings
                .AsNoTracking()
                .Where(scheduling => scheduling.Id == schedulingId)
                .ProjectTo<Scheduling>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }
    }
}
