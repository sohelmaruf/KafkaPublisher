using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KafkaPublisher
{
    class Program
    {    
        static void Main(string[] args)
        {
            Console.WriteLine("Publisher Started");
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, collection) =>
                {
                    //collection.AddHostedService<KafkaConsumerHostedService>();
                    collection.AddSingleton<IHostedService,KafkaProducerHostedService>();
                    //collection.AddHostedService<KafkaProducerHostedService>();
                });

    }

    public class KafkaProducerHostedService : IHostedService
    {
        private readonly ILogger<KafkaProducerHostedService> _logger;
        private readonly IProducer<Null, string> _producer;

        public KafkaProducerHostedService(ILogger<KafkaProducerHostedService> logger)
        {
            _logger = logger;
            var config = new ProducerConfig()
            {
                BootstrapServers = "localhost:9092"
            };
            _producer = new ProducerBuilder<Null, string>(config).Build();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (var i = 0; i < 100; ++i)
            {
                var value = $"Hello World {i}";
                _logger.LogInformation(value);
                await _producer.ProduceAsync("demo", new Message<Null, string>()
                {
                    Value = value
                }, cancellationToken);
            }

            _producer.Flush(TimeSpan.FromSeconds(10));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _producer?.Dispose();
            return Task.CompletedTask;
        }
    }



}
