using AutoMapper;
using FluentValidation;
using MediatR;
using VerticalApp.Data;
using VerticalApp.Models;

namespace VerticalApp.Features.Stocks;

public class AddStocks
{
    public class Command : IRequest<Result>
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public record class Result(int QuantityInStock);

    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Product, Result>();
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Quantity).GreaterThan(0);
        }
    }

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly ProductContext _db;
        private readonly IMapper _mapper;

        public Handler(ProductContext db, IMapper mapper)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var product = await _db.Products.FindAsync(new object[] { request.ProductId }, cancellationToken);
            if (product == null)
            {
                throw new ProductNotFoundException(request.ProductId);
            }

            product.QuantityInStock += request.Quantity;
            await _db.SaveChangesAsync(cancellationToken);

            return _mapper.Map<Result>(product);
        }
    }
}
