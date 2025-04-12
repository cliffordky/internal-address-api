using Core.Encryption;
using Newtonsoft.Json;

namespace Core.Models
{
    public record Address(Guid Id, Guid ConsumerId, Guid SubscriberId, string AddressLine1, string AddressLine2, string City, string State, string Zip, string ISOA3CountryCode, string AddressTypeId, DateTimeOffset RecordDate)
        : IHasEncryptionKey
    {
        [JsonIgnore]
        public string EncryptionKey => Id.ToString();
    }
}