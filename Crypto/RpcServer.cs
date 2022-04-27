using Business;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Crypto
{
    public class RpcServer : IDisposable
    {
        private const string QueueName = "CryptoQueueServer";
        private IConnection _connection;
        private IModel _channel;
        private bool _isDisposed;
        private readonly ICryptoService _cryptoService;
        public RpcServer(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }
        public void InitializeAndRun()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "CryptoQueueServer", durable: false, exclusive: false, autoDelete: false, arguments: null);
            //_channel.QueueDeclare(queue: "CryptoQueueClient", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.BasicQos(0, 1, false);
            var consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: QueueName, autoAck: false, consumer: consumer);
            consumer.Received += Consumer_Received;
        }
        private void Consumer_Received(object? sender, BasicDeliverEventArgs ea)
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var replyProps = _channel.CreateBasicProperties();
            replyProps.CorrelationId = ea.BasicProperties.CorrelationId;
            replyProps.ReplyTo = ea.BasicProperties.ReplyTo;
            var cryptographyRequest = JsonSerializer.Deserialize<CryptographyRequest>(message);
            var cryptographyResponse = new CryptographyResponse();
            if(cryptographyRequest.CryptographyProcessType == CryptographyProcessType.Encrypt)
            {
                cryptographyResponse= _cryptoService.Encrypt(cryptographyRequest);
            }
            else
            {
                cryptographyResponse = _cryptoService.Decrypt(cryptographyRequest);
            }
            if (cryptographyResponse.IsOk)
            {
                cryptographyResponse.RequestText = cryptographyRequest.Value;
                cryptographyResponse.CorrelationId = replyProps.CorrelationId;
                var response = JsonSerializer.Serialize<CryptographyResponse>(cryptographyResponse);
                var responseBody = Encoding.UTF8.GetBytes(response);
                _channel.BasicPublish(exchange: "", routingKey: "CryptoQueueClient", basicProperties: replyProps, body: responseBody);                
            }
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

        }
        private void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (disposing)
            {
                _channel.Close();
            }
            _isDisposed = true;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        ~RpcServer()
        {
            Dispose(false);
        }
    }
}
