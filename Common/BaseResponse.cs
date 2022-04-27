namespace Common
{
    public class BaseResponse
    {
        public bool IsOk { get; set; }
        public string ErrorMessage { get; set; }
        public string Result { get; set; }
    }
}
