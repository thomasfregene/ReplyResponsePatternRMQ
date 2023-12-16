using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

//Client

var factory = new ConnectionFactory { HostName= "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

var replyQueue = channel.QueueDeclare(queue: "", exclusive: true);
channel.QueueDeclare(queue: "request-queue", exclusive: false);
var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Reply Received: {message}");
};

channel.BasicConsume(queue: replyQueue.QueueName, autoAck: true, consumer: consumer);

//Publish request
var message = "Can I request a reply";
var body = Encoding.UTF8.GetBytes(message);

var properties = channel.CreateBasicProperties();
properties.ReplyTo= replyQueue.QueueName;
properties.CorrelationId = Guid.NewGuid().ToString();

channel.BasicPublish("", "request-queue", properties, body);

Console.WriteLine($"Sending Request: {properties.CorrelationId}");

Console.WriteLine($"Started Client");

Console.ReadKey();