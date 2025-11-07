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

        public async Task<Scheduling> Create(Scheduling scheduling)
        {
            var row = _mapper.Map<SchedulingTableRow>(scheduling);
            row.CustomerPlatformConfigurationId = scheduling.CustomerPlatformConfiguration.Id;
            _context.Schedulings.Add(row);
            await _context.SaveChangesAsync();
            return _mapper.Map<Scheduling>(row);
        }


        public async Task<List<Scheduling>> GetAllByConfigurationId(int configurationId)
        {
            return await _context.Schedulings
                .AsNoTracking()
                .Where(scheduling => scheduling.CustomerPlatformConfigurationId == configurationId)
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
                .Include(databaseScheduling => databaseScheduling.Parameters)
                .FirstOrDefault();

            if (row == null) throw new BadRequestException("Scheduling not found.");

            updateRowParametersIfTheyDifferFromIncoming(scheduling, row);

            _mapper.Map(scheduling, row);

            await _context.SaveChangesAsync();

            return _mapper.Map(row, scheduling);
        }

        private void updateRowParametersIfTheyDifferFromIncoming(Scheduling scheduling, SchedulingTableRow row)
        {
            var savedParameters = _mapper.Map<Parameters>(row.Parameters);

            var oldSavedParametersId = savedParameters.Id;
            savedParameters.Id = 0; // To avoid issues with comparing the IDs
            if (savedParameters != scheduling.Parameters)
                row.Parameters = _mapper.Map<ParametersTableRow>(scheduling.Parameters);
            else
            {
                savedParameters.Id = oldSavedParametersId;
                scheduling.Parameters = savedParameters;
            }
        }

        public async Task Delete(int schedulingId)
        {
            await _context.Schedulings
                .Where(scheduling => scheduling.Id == schedulingId)
                .ExecuteDeleteAsync();
        }
    }
}
