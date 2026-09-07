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

            PropagateGeneratedIds(configuration, row);
            return configuration;
        }

        private static void PropagateGeneratedIds(CustomerPlatformConfiguration configuration, CustomerPlatformConfigurationTableRow row)
        {
            configuration.Id = row.Id;

            if (configuration.Schedulings == null) return;

            for (int index = 0; index < configuration.Schedulings.Count; index++)
                configuration.Schedulings[index].Id = row.Schedulings[index].Id;
        }

        // Configurações ainda não confirmadas (integrações do Instagram que iniciaram a
        // autorização mas não voltaram no callback) não são visíveis para o cliente nem
        // contam nas métricas de conexões.
        public Task<List<CustomerPlatformConfiguration>> GetAllForCustomer(int customerId)
        {
            return _context.CustomerPlatformsConfiguration
                .AsNoTracking()
                .Where(configuration => configuration.CustomerId == customerId && configuration.IsConfirmed)
                .ProjectTo<CustomerPlatformConfiguration>(_mapper.ConfigurationProvider)
                .ToListAsync();


        }

        public async Task<CustomerPlatformConfiguration> GetById(int id)
        {
            return await _context.CustomerPlatformsConfiguration
                .AsNoTracking()
                .Where(configuration => configuration.Id == id && configuration.IsConfirmed)
                .ProjectTo<CustomerPlatformConfiguration>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task Delete(int id)
        {
            await _context.Schedulings
                .Where(s => s.CustomerPlatformConfigurationId == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.IsDeleted, true));

            await _context.CustomerPlatformsConfiguration
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(c => c.IsDeleted, true));
        }

        public async Task<CustomerPlatformConfiguration> Update(CustomerPlatformConfiguration configuration)
        {
            var row = await _context.CustomerPlatformsConfiguration
                .Where(c => c.Id == configuration.Id)
                .Include(c => c.Schedulings)
                .ThenInclude(s => s.Parameters)
                .Include(c => c.Schedulings)
                .ThenInclude(s => s.SchedulingStatus)
                .Include(c => c.Platform)
                .FirstOrDefaultAsync();

            if (row == null) return null;

            row.PlatformName = configuration.PlatformName;
            row.Url = configuration.Url;
            row.Username = configuration.Username;
            row.Password = configuration.Password;

            await _context.SaveChangesAsync();

            return _mapper.Map<CustomerPlatformConfiguration>(row);
        }

        public async Task<CustomerPlatformConfiguration> GetByInstagramState(string state)
        {
            return await _context.CustomerPlatformsConfiguration
                .AsNoTracking()
                .Where(configuration => configuration.InstagramState == state)
                .ProjectTo<CustomerPlatformConfiguration>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task SaveInstagramCredentials(int configurationId,
                                                   InstagramProfile profile,
                                                   InstagramAccessToken token,
                                                   List<string> permissions)
        {
            await _context.CustomerPlatformsConfiguration
                .Where(configuration => configuration.Id == configurationId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(configuration => configuration.InstagramState, (string)null)
                    .SetProperty(configuration => configuration.InstagramAccessToken, token.AccessToken)
                    .SetProperty(configuration => configuration.InstagramTokenExpiresAt, token.ExpiresAt())
                    .SetProperty(configuration => configuration.InstagramUserId, profile.UserId)
                    .SetProperty(configuration => configuration.InstagramUsername, profile.Username)
                    .SetProperty(configuration => configuration.IsConfirmed, true));

            await ReplaceInstagramPermissions(configurationId, permissions);
        }

        private async Task ReplaceInstagramPermissions(int configurationId, List<string> permissions)
        {
            if (permissions == null || permissions.Count == 0) return;

            await _context.InstagramPermissions
                .Where(permission => permission.CustomerPlatformConfigurationId == configurationId)
                .ExecuteDeleteAsync();

            _context.InstagramPermissions.AddRange(permissions.Select(permission => new InstagramPermissionTableRow
            {
                Permission = permission,
                CustomerPlatformConfigurationId = configurationId,
                CreatedAt = DateTime.Now
            }));

            await _context.SaveChangesAsync();
        }
    }
}
