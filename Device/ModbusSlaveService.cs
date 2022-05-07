using System.Net;
using System.Net.Sockets;
using Modbus.Data;
using Modbus.Device;

namespace Device
{
    public class ModbusSlaveService : BackgroundService
    {
        private readonly ILogger<ModbusSlaveService> _logger;
        private readonly TcpListener _slaveTcpListener;
        private readonly Timer _readTimer;
        private readonly ModbusSlave _slave;
        public ModbusSlaveService(ILogger<ModbusSlaveService> logger)
        {
            _logger = logger;
            byte slaveId = 1;
            int port = 502;
            IPAddress address = IPAddress.Any;

            // create and start the TCP slave
            _slaveTcpListener = new TcpListener(address, port);
            _slaveTcpListener.Start();
            _slave = ModbusTcpSlave.CreateTcp(slaveId, _slaveTcpListener);
            _slave.DataStore = DataStoreFactory.CreateDefaultDataStore();
            _slave.ListenAsync();
            _readTimer = new Timer(ReadCurrentValue, null, 0, 1000);
            _logger.LogInformation($"Modbus Server Started");
        }

        private uint _lastValue;
        private void ReadCurrentValue(object? state)
        {
            var currentValue = _slave.DataStore.HoldingRegisters[2];
            if (_lastValue != currentValue)
            {
                _logger.LogInformation($"Current Value change to {currentValue}");
                _lastValue = currentValue;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
        }

        public override void Dispose()
        {
            _logger.LogError($"Modbus Server Dispose");
            _readTimer.Dispose();
            _slaveTcpListener.Stop();
        }
    }
}