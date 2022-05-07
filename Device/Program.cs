using Device;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder => builder.AddSeq());
        //注入模拟设备的服务
        services.AddHostedService<ModbusSlaveService>();
        services.AddLogging(builder => builder.AddSeq());
    })
    .Build();

await host.RunAsync();
