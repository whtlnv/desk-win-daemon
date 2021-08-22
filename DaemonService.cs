using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Server;

namespace iotBridge {
  public class DaemonService: IHostedService, IDisposable {
    private readonly ILogger _logger;
    private readonly IOptions<DaemonConfig> _config;
    private IMqttServer _mqttServer;

    public DaemonService(ILogger<DaemonService> logger, IOptions<DaemonConfig> config) {
      _logger = logger;
      _config = config;
    }

    public Task StartAsync(CancellationToken cancellationToken) {
      var port = _config.Value.Port;
      _logger.LogInformation("Starting IoT Daemon on port: " + port);

      _mqttServer = new MqttFactory().CreateMqttServer();

      var optionsBuilder = new MqttServerOptionsBuilder()
          .WithConnectionBacklog(1000)
          .WithDefaultEndpointPort(port)
          .WithConnectionValidator()
          .WithSubscriptionInterceptor()
          .WithUnsubscriptionInterceptor()
          .WithApplicationMessageInterceptor()
          // .WithUndeliveredMessageInterceptor()
          // .WithPersistentSessions()
          ;
      
      return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
      _logger.LogInformation("Stopping IoT Daemon");
      return _mqttServer.StopAsync();
    }

    public void Dispose() {
      _logger.LogInformation("Disposing...");
    }

    // MQTT
    private void _ClientConnected(object sender, MqttServerClientConnectedEventArgs e) {
      _logger.LogInformation(e.ClientId + " Connected");
    }

    private void _ClientDisconnected(object sender, MqttServerClientDisconnectedEventArgs e) {
      _logger.LogInformation(e.ClientId + " Disconnected");
    }

    private void _ClientSubscribed(object sender, MqttServerClientSubscribedTopicEventArgs e) {
      _logger.LogInformation(e.ClientId + " subscribed to " + e.TopicFilter);
    }

      private void _ClientUnsubscribed(object sender, MqttServerClientUnsubscribedTopicEventArgs e) {
      _logger.LogInformation(e.ClientId + " unsubscribed to " + e.TopicFilter);
    }

    private void _ApplicationMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e) {
      _logger.LogInformation(e.ClientId + " published message to topic " + e.ApplicationMessage.Topic);
    }
  }
}