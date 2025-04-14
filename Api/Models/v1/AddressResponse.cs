namespace Api.Models.v1
{
    public class AddressResponse
    {
        public Guid Id { get; set; }
        public Guid ConsumerId { get; set; }
        public Guid SubscriberId { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string ISOA3CountryCode { get; set; }
        public string AddressTypeCode { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public DateTimeOffset RecordDate { get; set; }
    }
}