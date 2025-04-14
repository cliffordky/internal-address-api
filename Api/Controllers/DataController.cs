using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Coravel.Cache.Interfaces;
using Marten;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DataController : ControllerBase
    {
        private readonly ILogger<DataController> _logger;
        private readonly IDocumentStore _store;
        private readonly ICache _cache;

        public DataController(ILogger<DataController> logger, IDocumentStore store, ICache cache)
        {
            _logger = logger;
            _store = store;
            _cache = cache;
        }

        [Authorize(Policy = "write")]
        [TranslateResultToActionResult]
        [HttpPost("address")]
        public async Task<Result<Models.v1.AddressResponse>> AddAddress(Models.v1.AddressRequest request)
        {
            try
            {
                string hash = Core.Encryption.Hash.GetHashString(
                    request.ConsumerId.ToString() +
                    request.SubscriberId.ToString() +
                    request.AddressLine1 +
                    request.AddressLine2 +
                    request.City +
                    request.State +
                    request.Zip +
                    request.ISOA3CountryCode +
                    request.AddressTypeCode +
                    request.RecordDate);

                await using var session = _store.LightweightSession();
                var existing = await session.Query<Core.Models.Address>().SingleOrDefaultAsync(x => x.Hash == hash);
                if (existing != null)
                {
                    return Result<Models.v1.AddressResponse>.Error("Address already exists");
                }

                var address = new Core.Models.Address(
                        Guid.NewGuid(),
                        request.ConsumerId,
                        request.SubscriberId,
                        request.AddressLine1,
                        request.AddressLine2,
                        request.City,
                        request.State,
                        request.Zip,
                        request.ISOA3CountryCode,
                        request.AddressTypeCode,
                        request.RecordDate,
                        hash);

                session.Store(address);
                await session.SaveChangesAsync();

                return Result<Models.v1.AddressResponse>.Success(new Models.v1.AddressResponse
                {
                    Id = address.Id,
                    ConsumerId = address.ConsumerId,
                    SubscriberId = address.SubscriberId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    Zip = address.Zip,
                    ISOA3CountryCode = address.ISOA3CountryCode,
                    AddressTypeCode = address.AddressTypeCode,
                    RecordDate = address.RecordDate
                });
            }
            catch (Exception Ex)
            {
                return Result<Models.v1.AddressResponse>.Error(Ex.Message);
            }
        }

        [Authorize(Policy = "read")]
        [TranslateResultToActionResult]
        [HttpGet("addresses")]
        public async Task<Result<List<Models.v1.AddressResponse>>> GetAddressesForConsumer(Guid ConsumerId)
        {
            try
            {
                await using var session = _store.LightweightSession();
                var addresses = await session.Query<Core.Models.Address>().Where(x => x.ConsumerId == ConsumerId).ToListAsync();

                return Result<List<Models.v1.AddressResponse>>.Success(addresses.Select(
                    x => new Models.v1.AddressResponse
                    {
                        Id = x.Id,
                        ConsumerId = x.ConsumerId,
                        SubscriberId = x.SubscriberId,
                        AddressLine1 = x.AddressLine1,
                        AddressLine2 = x.AddressLine2,
                        City = x.City,
                        State = x.State,
                        Zip = x.Zip,
                        ISOA3CountryCode = x.ISOA3CountryCode,
                        AddressTypeCode = x.AddressTypeCode,
                        RecordDate = x.RecordDate
                    }).ToList());
            }
            catch (Exception Ex)
            {
                return Result<List<Models.v1.AddressResponse>>.Error(Ex.Message);
            }
        }
    }
}