using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Mirra_Portal_API.Database.DBEntities;
using Mirra_Portal_API.Database.Repositories.Interfaces;
using Mirra_Portal_API.Model;

namespace Mirra_Portal_API.Database.Repositories
{
    public class CustomerRepository : DefaultRepository, ICustomerRepository
    {
        public CustomerRepository(DatabaseContext context, IMapper mapper) : base(context, mapper)
        {
        }

        public async Task<Customer> Create(Customer customer)
        {
            var row = _mapper.Map<CustomerTableRow>(customer);

            _context.Customers.Add(row);
            await _context.SaveChangesAsync();
            customer.Id = row.Id;
            return customer;
        }

        public async Task<Customer> GetByEmail(string email)
        {

            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.Email == email)
                .ProjectTo<Customer>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

        }


        public async Task<Customer> GetByConfigurationId(int configurationId)
        {
            var row = await _context.Customers
                .AsNoTracking()
                .Where(customer => customer.PlatformsConfigurations.Any(platformConfiguration =>
                        platformConfiguration.Id == configurationId))
                .Include(customer => customer.SubscriptionPlan)
                .FirstOrDefaultAsync();

            return _mapper.Map<Customer>(row);
        }


        public async Task<Customer> Update(Customer customer)
        {
            var row = await _context.Customers
                .Where(databaseCustomer => databaseCustomer.Id == customer.Id)
                .FirstOrDefaultAsync();

            if (row == null) return customer;

            _mapper.Map(customer, row);
            await _context.SaveChangesAsync();

            return _mapper.Map(row, customer);
        }
    }
}
