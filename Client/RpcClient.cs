using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Client
{
    internal class RpcClient : IDisposable
    {
        private IConnection _connection;
        private IModel _channel;
        private string _responseQueueName;
        private bool _isDisposed;
        private readonly Dictionary<string, CryptographyRequest> _requests;
        public RpcClient(Dictionary<string, CryptographyRequest> requests)
        {
            _requests = requests;
        }
        public void Initialiaze()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: "CryptoQueueClient", durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: "CryptoQueueClient", autoAck: false, consumer: consumer);
            consumer.Received += Consumer_Received;
        }
        private void Consumer_Received(object? sender, BasicDeliverEventArgs args)
        {
            var body = args.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var response = JsonSerializer.Deserialize<CryptographyResponse>(message);
            if (_requests.ContainsKey(response.CorrelationId))
            {
                var request = _requests[response.CorrelationId];
                Console.WriteLine(string.Format("RequestType =>{0} requestValue =>{1} responseValue =>{2}", request.CryptographyProcessType.ToString(), request.Value, response.Result));                
            }
            _channel.BasicAck(deliveryTag: args.DeliveryTag, multiple: false);
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
        ~RpcClient()
        {
            Dispose(false);
        }
    }
}
