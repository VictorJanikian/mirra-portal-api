using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories
{
    public class CustomerContentPlatformConfigurationRepository : DefaultRepository, ICustomerContentPlatformConfigurationRepository
    {
        public CustomerContentPlatformConfigurationRepository(DatabaseContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<CustomerContentPlatformConfiguration> Create(CustomerContentPlatformConfiguration configuration)
        {
            var row = _mapper.Map<CustomerContentPlatformConfigurationTableRow>(configuration);
            _context.CustomerContentPlatformsConfiguration.Add(row);
            await _context.SaveChangesAsync();
            configuration.Id = row.Id;
            return configuration;
        }

        public async Task<CustomerContentPlatformConfiguration> GetById(int id)
        {
            return await _context.CustomerContentPlatformsConfiguration
                .AsNoTracking()
                .Where(configuration => configuration.Id == id)
                .ProjectTo<CustomerContentPlatformConfiguration>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();


        }
    }
}
