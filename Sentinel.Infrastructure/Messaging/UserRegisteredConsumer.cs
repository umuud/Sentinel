using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Sentinel.Application.Events;

namespace Sentinel.Infrastructure.Messaging;

public class UserRegisteredConsumer : BackgroundService
{
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private readonly IConnection _connection;
    private IModel? _channel;
    private const string ExchangeName = "sentinel.events";
    private const string QueueName = "user.registered";
    private const string RoutingKey = "UserRegisteredEvent";

    public UserRegisteredConsumer(IConnection connection, ILogger<UserRegisteredConsumer> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        _channel.QueueBind(
            queue: QueueName,
            exchange: ExchangeName,
            routingKey: RoutingKey);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (_, ea) =>
        {
            try
            {
                var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(body);

                if (@event is not null)
                    _logger.LogInformation(
                        "Yeni kullanıcı kaydoldu — Id: {AccountId}, Username: {Username}, Email: {Email}, Tarih: {RegisteredAt}",
                        @event.AccountId,
                        @event.Username,
                        @event.Email,
                        @event.RegisteredAt);

                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UserRegisteredEvent işlenirken hata oluştu");
                _channel.BasicNack(ea.DeliveryTag, false, requeue: true);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        base.Dispose();
    }
}