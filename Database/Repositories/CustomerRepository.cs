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
    }
}
