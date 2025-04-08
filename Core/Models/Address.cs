using Core.Encryption;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public record Address(Guid Id, Guid ConsumerId, string AddressLine1, string AddressLine2, string City, string State, string Zip, string ISOA3CountryCode, DateTimeOffset RecordDate)
        : IHasEncryptionKey
    {
        [JsonIgnore]
        public string EncryptionKey => Id.ToString();
    }
}