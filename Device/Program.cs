using Device;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddLogging(builder => builder.AddSeq());
        //ע��ģ���豸�ķ���
        services.AddHostedService<ModbusSlaveService>();
        services.AddLogging(builder => builder.AddSeq());
    })
    .Build();

await host.RunAsync();
