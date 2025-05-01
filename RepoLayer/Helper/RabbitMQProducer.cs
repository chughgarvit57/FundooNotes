using System.Text;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace RepoLayer.Helper
{
    public class RabbitMQProducer
    {
        private readonly IConfiguration _config;
        public RabbitMQProducer(IConfiguration config)
        {
            _config = config;
        }
        public async Task PublishMessageAsync(object message)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _config["RabbitMQ:HostName"],
                UserName = _config["RabbitMQ:UserName"],
                Password = _config["RabbitMQ:Password"]
            };

            await using var connection = await factory.CreateConnectionAsync();
            await using var channel = await connection.CreateChannelAsync(); 

            await channel.QueueDeclareAsync(
                queue: _config["RabbitMQ:QueueName"],
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: _config["RabbitMQ:QueueName"],
                mandatory: false,
                body: body
            );
        }
    }
}
