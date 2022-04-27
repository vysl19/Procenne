using Common;
using EasyCaching.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Gateway.Business
{
    public class GateWayService : IGateWayService
    {
        private IEasyCachingProvider _easyCachingProvider;
        private IEasyCachingProviderFactory _easyCachingProviderFactory;
        private const string QueueName = "CryptoQueueServer";
        private IConnection _connection;
        private IModel _channel;
        public GateWayService(IEasyCachingProviderFactory easyCachingProviderFactory)
        {
            _easyCachingProviderFactory = easyCachingProviderFactory;
            _easyCachingProvider = _easyCachingProviderFactory.GetCachingProvider("redis1");

        }

        public TokenResponse GetToken(TokenRequest request)
        {
            var tokenResponse = new TokenResponse();
            try
            {
                var isManuelCacheInserted = _easyCachingProvider.Get<bool>("IsApiKeyPassInsertedAlready");
                if (!isManuelCacheInserted.HasValue)
                {
                    using (var r = new StreamReader("keys.json"))
                    {
                        string json = r.ReadToEnd();
                        var items = JsonSerializer.Deserialize<ApiKeyPassInfo>(json).ApiKeyPasses;
                        foreach (var item in items)
                        {
                            _easyCachingProvider.Set(item.ApiKey, item.ApiPass, TimeSpan.FromDays(1));
                        }
                        _easyCachingProvider.Set("IsApiKeyPassInsertedAlready", true, TimeSpan.FromDays(1));
                    }
                }
                var apiKeyCache = _easyCachingProvider.Get<string>(request.ApiKey);
                if (!apiKeyCache.HasValue || string.IsNullOrEmpty(apiKeyCache.Value))
                {
                    tokenResponse.ErrorMessage = string.Format("There is no api key for {0} in cache", request.ApiKey);
                    return tokenResponse;
                }
                if (apiKeyCache.Value != request.ApiPass)
                {
                    tokenResponse.ErrorMessage = string.Format("Requested pass({0}) is different form in cache", request.ApiPass);
                    return tokenResponse;
                }
                var guid = Guid.NewGuid().ToString();
                _easyCachingProvider.Set(guid, 1, TimeSpan.FromMinutes(30));
                tokenResponse.IsOk = true;
                tokenResponse.Result = guid;
            }
            catch (Exception e)
            {
                tokenResponse.ErrorMessage = e.Message;
            }
            return tokenResponse;
        }

        public CryptographyResponse SendCryptoProcess(CryptographyRequest request)
        {
            var response = new CryptographyResponse();
            try
            {
                var token = _easyCachingProvider.Get<int>(request.Token);
                if (token.HasValue && token.Value > 0)
                {
                    InitializeRabbitMQ();
                    var replyProps = _channel.CreateBasicProperties();
                    replyProps.ReplyTo = "CryptoQueueServer";                    
                    replyProps.CorrelationId = request.CorrelationId;
                    var publishBody = JsonSerializer.Serialize<CryptographyRequest>(request);
                    _channel.BasicPublish(exchange: "", routingKey: replyProps.ReplyTo, basicProperties: replyProps, body: Encoding.UTF8.GetBytes(publishBody));
                    response.IsOk = true;                    
                }
                else
                {
                    response.ErrorMessage = "Invalid Token";
                }
            }
            catch (Exception e)
            {
                response.ErrorMessage = e.Message;
            }
            return response;

        }
        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: QueueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.BasicQos(0, 1, false);
        }
    }
}
