using System.Net;
using System.Net.Sockets;
using Modbus.Data;
using Modbus.Device;

namespace IoTGatewayService
{
    public class DeviceClient : IDisposable
    {
        private readonly ILogger<DeviceClient> _logger;
        private readonly TcpClient _clientTcp;
        private readonly ModbusMaster _master;
        public DeviceClient(ILogger<DeviceClient> logger)
        {
            _logger = logger;
            IPAddress address = IPAddress.Any;

            // create and start the TCP slave
            _clientTcp = new TcpClient("localhost", 502);
            _master = ModbusIpMaster.CreateIp(_clientTcp);
            _logger.LogInformation($"Gateway Started");
        }

        public SetReply WriteValue(ushort address, ushort value)
        {
            var response = new SetReply { Message = "成功", Success = true };
            if (_clientTcp.Connected)
                _master.WriteSingleRegister(1, address, value);
            else
            {
                response.Success = false;
                response.Message = "连接失败";
            }
            return response;
        }

        public void Dispose()
        {
            _logger.LogError($"Gateway Dispose");
        }
    }
}
