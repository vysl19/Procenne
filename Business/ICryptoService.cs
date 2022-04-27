using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business
{
    public interface ICryptoService
    {
        CryptographyResponse Encrypt(CryptographyRequest request);
        CryptographyResponse Decrypt(CryptographyRequest request);
    }
}
