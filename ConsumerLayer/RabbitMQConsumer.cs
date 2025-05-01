using System.Net.Mail;
using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ConsumerLayer.Consumer
{
    public class RabbitMQConsumer
    {
        private readonly IConfiguration _config;

        public RabbitMQConsumer(IConfiguration config)
        {
            _config = config;
        }

        public async Task ConsumeMessagesAsync()
        {
            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _config["RabbitMQ:HostName"],
                    UserName = _config["RabbitMQ:UserName"],
                    Password = _config["RabbitMQ:Password"]
                };

                var connection = await factory.CreateConnectionAsync();
                var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclareAsync(
                    queue: _config["RabbitMQ:QueueName"],
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Received message: {message}");

                        var json = JObject.Parse(message);
                        string to = json["to"]?.ToString();
                        string subject = json["subject"]?.ToString();
                        string bodyContent = json["body"]?.ToString();

                        if (!string.IsNullOrEmpty(to))
                        {
                            await SendEmailAsync(to, subject, bodyContent);
                            Console.WriteLine($"Email sent to {to}!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing message: {ex.Message}");
                    }
                };

                await channel.BasicConsumeAsync(
                    queue: _config["RabbitMQ:QueueName"],
                    autoAck: true,
                    consumer: consumer
                );

                Console.WriteLine("Consumer started. Waiting for messages...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting up RabbitMQ consumer: {ex.Message}");
            }
            Console.ReadLine();
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var fromEmail = _config["EmailSettings:FromEmail"];
            var password = _config["EmailSettings:AppPassword"];
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);

            using (var message = new MailMessage(fromEmail, to, subject, body))
            {
                message.IsBodyHtml = true;

                using (var smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(fromEmail, password);
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(message);
                }
            }
        }
    }
}