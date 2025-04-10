﻿using Ardalis.Result;
using Ardalis.Result.AspNetCore;
using Coravel.Cache.Interfaces;
using Marten;
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

        [TranslateResultToActionResult]
        [HttpPost("address")]
        public async Task<Result<Models.v1.AddressResponse>> AddAddress(Models.v1.AddressRequest request)
        {
            try
            {
                var address = new Core.Models.Address(
                        Guid.NewGuid(),
                        request.ConsumerId,
                        request.AddressLine1,
                        request.AddressLine2,
                        request.City,
                        request.State,
                        request.Zip,
                        request.ISOA3CountryCode,
                        request.AddressTypeId.ToString(),
                        request.RecordDate
                    );

                await using var session = _store.LightweightSession();
                session.Store(address);
                await session.SaveChangesAsync();

                return Result<Models.v1.AddressResponse>.Success(new Models.v1.AddressResponse
                {
                    Id = address.Id,
                    ConsumerId = address.ConsumerId,
                    AddressLine1 = address.AddressLine1,
                    AddressLine2 = address.AddressLine2,
                    City = address.City,
                    State = address.State,
                    Zip = address.Zip,
                    ISOA3CountryCode = address.ISOA3CountryCode,
                    AddressTypeId = Int32.Parse(address.AddressTypeId),
                    RecordDate = address.RecordDate
                });
            }
            catch (Exception Ex)
            {
                return Result<Models.v1.AddressResponse>.Error(Ex.Message);
            }
        }

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
                        AddressLine1 = x.AddressLine1,
                        AddressLine2 = x.AddressLine2,
                        City = x.City,
                        State = x.State,
                        Zip = x.Zip,
                        ISOA3CountryCode = x.ISOA3CountryCode,
                        AddressTypeId = Int32.Parse(x.AddressTypeId),
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