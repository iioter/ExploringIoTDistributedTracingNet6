using Grpc.Core;

namespace IoTGatewayService.Services
{
    public class GatewayService : Greeter.GreeterBase
    {
        private readonly ILogger<GatewayService> _logger;
        private readonly DeviceClient _gateway;
        public GatewayService(ILogger<GatewayService> logger, DeviceClient gateway)
        {
            _logger = logger;
            _gateway = gateway;
        }

        public override Task<SetReply> GatewaySetValue(SetRequest request, ServerCallContext context)
        {
            var replay = _gateway.WriteValue(1, (ushort)request.Value);
            _logger.LogInformation(
                $"request:{request.Value},replay:Success[{replay.Success}],Message[{replay.Message}]");
            return Task.FromResult(replay);
        }
    }
}