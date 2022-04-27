using Common;
namespace Gateway.Business
{
    public interface IGateWayService
    {
        TokenResponse GetToken(TokenRequest request);
        CryptographyResponse SendCryptoProcess(CryptographyRequest request);
    }
}
