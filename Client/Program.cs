using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var tokenResponse = GetToken();
            var dictionary = new Dictionary<string, CryptographyRequest>();

            if (tokenResponse.IsOk)
            {
                var encryptRequest = new CryptographyRequest()
                {
                    CryptographyProcessType = CryptographyProcessType.Encrypt,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Value = "veysel",
                    Token = tokenResponse.Result
                };
                var decryptRequest = new CryptographyRequest()
                {
                    CryptographyProcessType = CryptographyProcessType.Decrypt,
                    CorrelationId = Guid.NewGuid().ToString(),
                    Value = "Vj4udY1QLvkg21aFAsf57g==",
                    Token = tokenResponse.Result
                };
                dictionary.Add(encryptRequest.CorrelationId, encryptRequest);
                dictionary.Add(decryptRequest.CorrelationId, decryptRequest);
                var client = new RpcClient(dictionary);
                client.Initialiaze();
                RequestEncrypt(encryptRequest);
                RequestDecrypt(decryptRequest);
            }
            Console.Read();
        }
        public static TokenResponse GetToken()
        {
            var tokenResponse = new TokenResponse();
            var url = "https://localhost:44375/gateway/token";
            var request = new TokenRequest()
            {
                ApiKey = "test",
                ApiPass = "1234"
            };
            string inputJson = JsonSerializer.Serialize(request);
            byte[] bytes = Encoding.UTF8.GetBytes(inputJson);

            var response = CallRestService(url, bytes);
            tokenResponse = JsonSerializer.Deserialize<TokenResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return tokenResponse;
        }
        public static string CallRestService(string url, byte[] request)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(new Uri(url));
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            ServicePointManager.ServerCertificateValidationCallback =
                (sender, certificate, chain, errors) => true;

            using (Stream stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(request, 0, request.Length);
                stream.Close();
            }

            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                using (Stream stream = httpWebResponse.GetResponseStream())
                {
                    return (new StreamReader(stream)).ReadToEnd();
                }
            }
        }
        public static CryptographyResponse RequestEncrypt(CryptographyRequest request)
        {            
            var url = "https://localhost:44375/gateway/makecryptoprocess";
            string inputJson = JsonSerializer.Serialize(request);
            byte[] bytes = Encoding.UTF8.GetBytes(inputJson);

            var response = CallRestService(url, bytes);
            return JsonSerializer.Deserialize<CryptographyResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        public static CryptographyResponse RequestDecrypt(CryptographyRequest request)
        {
            var url = "https://localhost:44375/gateway/makecryptoprocess";
            string inputJson = JsonSerializer.Serialize(request);
            byte[] bytes = Encoding.UTF8.GetBytes(inputJson);

            var response = CallRestService(url, bytes);
            return JsonSerializer.Deserialize<CryptographyResponse>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        public static void WriteResponse()
        {
            
            
        }
    }
}
