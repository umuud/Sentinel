using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Sentinel.Application.Interfaces;

namespace Sentinel.Infrastructure.Messaging;

public class RabbitMqEventBus : IEventBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqEventBus> _logger;
    private const string ExchangeName = "sentinel.events";

    public RabbitMqEventBus(IConnection connection, ILogger<RabbitMqEventBus> logger)
    {
        _connection = connection;
        _logger = logger;
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(
            exchange: ExchangeName,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var routingKey = typeof(T).Name;

        try
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(@event));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: routingKey,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Event publish edildi — Type: {EventType}, RoutingKey: {RoutingKey}",
                typeof(T).Name, routingKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Event publish başarısız — Type: {EventType}", typeof(T).Name);
            throw;
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}