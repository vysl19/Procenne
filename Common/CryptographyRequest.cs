namespace Common
{
    public class CryptographyRequest
    {
        public string Token { get; set; }
        public CryptographyProcessType CryptographyProcessType { get; set; }
        public string Value { get; set; }
        public string CorrelationId { get; set; }
    }
}
