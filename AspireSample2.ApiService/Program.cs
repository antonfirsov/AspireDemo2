using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddHttpClient();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ApiPartyService>();
builder.Services.AddHostedService(provider => provider.GetService<ApiPartyService>()!);
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();
app.MapControllers();

app.Run();


[ApiController]
[Route("[controller]")]
public class DelayController : ControllerBase
{
    private ILogger<DelayController> _logger;
    private ApiPartyService _party;

    public DelayController(ApiPartyService party, ILogger<DelayController> logger)
    {
        _party = party;
        _logger = logger;
    }

    [HttpGet]
    public int Get()
    {
        return _party.Delay;
    }

    [HttpPut]
    public void Put([FromBody] int delay)
    {
        _party.Delay = Math.Min(Math.Max(delay, 30), 60_000);
        _logger.Log(LogLevel.Information, $"Delay set to {_party.Delay}.");
    }
}

[ApiController]
[Route("[controller]")]
public class StartStopController : ControllerBase
{
    private ILogger<DelayController> _logger;
    private ApiPartyService _party;

    public StartStopController(ApiPartyService party, ILogger<DelayController> logger)
    {
        _party = party;
        _logger = logger;
    }

    [HttpPut]
    public void Start()
    {
        _logger.Log(LogLevel.Information, "START");
    }

    [HttpPut]
    public void Stop()
    {
        _logger.Log(LogLevel.Information, "STOP");
    }
}


public class ApiPartyService : BackgroundService
{
    private readonly ILogger<ApiPartyService> _logger;
    private IHttpClientFactory _httpClientFactory;

    public int Delay { get; set; } = 10_000;

    private static readonly string[] Apis = [
        "https://example.com/",
        "https://httpbin.org/get",
        "https://dog.ceo/api/breeds/list/all",
        "https://www.anapioficeandfire.com/api/characters",
        "https://api.ipify.org?format=json",
        "https://official-joke-api.appspot.com/random_joke",
        "https://chroniclingamerica.loc.gov/search/titles/results/?terms=oakland&format=json&page=5",
        "https://yesno.wtf/api",
        "https://api.sunrise-sunset.org/json?lat=36.7201600&lng=-4.4203400",
        "http://universities.hipolabs.com/search?name=middle",
        "https://haveibeenpwned.com/api/v2/breaches",
    ];

    public ApiPartyService(ILogger<ApiPartyService> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory; 
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ApiPartyService running.");

        while (!stoppingToken.IsCancellationRequested)
        {
            int idx = Random.Shared.Next(0, Apis.Length);
            string uri = Apis[idx];

            HttpClient client = _httpClientFactory.CreateClient();
            _logger.LogInformation($"Sending request to {uri}");
            _ = client.GetStringAsync(uri, stoppingToken); // Fire and forget
            _logger.LogInformation($"Got response from {uri}");

            await Task.Delay(Random.Shared.Next(Delay), stoppingToken);
        }
    }
}