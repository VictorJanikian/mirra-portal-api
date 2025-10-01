using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Exceptions;
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

        public async Task<Scheduling> Update(Scheduling scheduling)
        {
            var row = _context.Schedulings
                .Where(databaseScheduling => databaseScheduling.Id == scheduling.Id)
                .Include(scheduling => scheduling.Parameters)
                .FirstOrDefault();

            if (row == null) throw new BadRequestException("Scheduling not found.");

            var savedParameters = _mapper.Map<Parameters>(row.Parameters);

            savedParameters.Id = 0; // To avoid issues with comparing the IDs
            if (savedParameters != scheduling.Parameters)
                await createNewParametersRow(scheduling, row);

            _mapper.Map(scheduling, row);

            await _context.SaveChangesAsync();

            return _mapper.Map(row, scheduling);
        }

        private async Task createNewParametersRow(Scheduling scheduling, SchedulingTableRow row)
        {
            var newParametersRow = _mapper.Map<ParametersTableRow>(scheduling.Parameters);
            row.Parameters = newParametersRow;
            _context.Parameters.Add(newParametersRow);
            await _context.SaveChangesAsync();
            row.ParametersId = newParametersRow.Id;
        }
    }
}
