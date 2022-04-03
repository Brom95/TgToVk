using TgToVkPoster;

var host = Host.CreateDefaultBuilder(args).ConfigureServices((ctx, services) =>
    {
        services.AddSingleton(ctx.Configuration.GetSection("TgToVk").Get<TgToVkConfiguration>());
        
        services.AddSingleton(new HttpClient());
        services.AddHostedService<Worker>();
    })
    .Build();


await host.RunAsync();
