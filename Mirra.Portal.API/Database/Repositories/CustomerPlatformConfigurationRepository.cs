using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories
{
    public class CustomerPlatformConfigurationRepository : DefaultRepository, ICustomerPlatformConfigurationRepository
    {
        public CustomerPlatformConfigurationRepository(DatabaseContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<CustomerPlatformConfiguration> Create(CustomerPlatformConfiguration configuration)
        {
            var row = _mapper.Map<CustomerPlatformConfigurationTableRow>(configuration);
            if (configuration.Schedulings != null)
                row.Schedulings = _mapper.Map<List<SchedulingTableRow>>(configuration.Schedulings);

            _context.CustomerPlatformsConfiguration.Add(row);
            await _context.SaveChangesAsync();
            configuration.Id = row.Id;
            return configuration;
        }

        public Task<List<CustomerPlatformConfiguration>> GetAllForCustomer(int customerId)
        {
            return _context.CustomerPlatformsConfiguration
                .AsNoTracking()
                .Where(configuration => configuration.CustomerId == customerId)
                .ProjectTo<CustomerPlatformConfiguration>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<CustomerPlatformConfiguration> GetById(int id)
        {
            return await _context.CustomerPlatformsConfiguration
                .AsNoTracking()
                .Where(configuration => configuration.Id == id)
                .ProjectTo<CustomerPlatformConfiguration>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task Delete(int id)
        {
            await _context.Schedulings
                .Where(s => s.CustomerPlatformConfigurationId == id)
                .ExecuteDeleteAsync();

            await _context.CustomerPlatformsConfiguration
                .Where(c => c.Id == id)
                .ExecuteDeleteAsync();
        }

        public async Task<CustomerPlatformConfiguration> Update(CustomerPlatformConfiguration configuration)
        {
            var row = await _context.CustomerPlatformsConfiguration
                .Where(c => c.Id == configuration.Id)
                .FirstOrDefaultAsync();

            if (row == null) return null;

            row.PlatformName = configuration.PlatformName;
            row.Url = configuration.Url;
            row.Username = configuration.Username;
            row.Password = configuration.Password;

            await _context.SaveChangesAsync();

            return configuration;
        }
    }
}
