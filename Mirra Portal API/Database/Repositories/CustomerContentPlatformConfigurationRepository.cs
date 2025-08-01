using AutoMapper;
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

        public async Task<CustomerPlatformConfiguration> Create(CustomerPlatformConfiguration configuration)
        {
            var row = _mapper.Map<CustomerContentPlatformConfigurationTableRow>(configuration);
            _context.CustomerContentPlatforms.Add(row);
            await _context.SaveChangesAsync();
            configuration.Id = row.Id;
            return configuration;
        }
    }
}
