using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Business
{
    public class TripleDESCryptoService : ICryptoService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        public TripleDESCryptoService(byte[] key, byte[] iv)
        {
            _key = key;
            _iv = iv;
        }
        public CryptographyResponse Encrypt(CryptographyRequest request)
        {
            var response = new CryptographyResponse();
            try
            {
                byte[] encrypted;
                using (SymmetricAlgorithm tdes = new TripleDESCryptoServiceProvider())
                {
                    ICryptoTransform encryptor = tdes.CreateEncryptor(_key, _iv);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter sw = new StreamWriter(cs, Encoding.Unicode))
                                sw.Write(request.Value);
                            encrypted = ms.ToArray();
                        }
                    }
                }
                response.IsOk = true;
                response.Result = Convert.ToBase64String(encrypted);
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
            }

            return response;
        }
        public CryptographyResponse Decrypt(CryptographyRequest request)
        {
            var response = new CryptographyResponse();
            try
            {
                using (SymmetricAlgorithm tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Padding = PaddingMode.Zeros;
                    ICryptoTransform decryptor = tdes.CreateDecryptor(_key, _iv);
                    using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(request.Value)))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader reader = new StreamReader(cs, Encoding.Unicode))
                            {                            
                                response.Result = reader.ReadToEnd();
                                response.IsOk = true;
                            }

                        }
                    }
                }
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
            }

            return response;
        }
    }
}
