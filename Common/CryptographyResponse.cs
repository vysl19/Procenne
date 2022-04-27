namespace Common
{
    public class CryptographyResponse : BaseResponse
    {
        public string RequestText { get; set; }
        public string CorrelationId { get; set; }
    }
}
