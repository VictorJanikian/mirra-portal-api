using AutoMapper;

namespace Mirra_Portal_API.Database.Repositories
{
    public class DefaultRepository
    {
        protected DatabaseContext _context;
        protected readonly IMapper _mapper;

        public DefaultRepository(DatabaseContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }
    }
}
