namespace AspireSample2.Web;

public class ApiClient(HttpClient httpClient)
{
    public async Task<int> GetDelayAsync(CancellationToken cancellationToken = default)
    {
        string value = await httpClient.GetStringAsync("/delay", cancellationToken);
        return int.TryParse(value, out int delay) ? delay : -1;
    }

    public async Task SetDelayAsync(int delay, CancellationToken cancellationToken = default)
    {
        JsonContent content = JsonContent.Create(delay);
        HttpResponseMessage response = await httpClient.PutAsync("/delay", content, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task Start(CancellationToken cancellationToken = default)
    {
        await httpClient.PutAsync("/startstop/start", null, cancellationToken);
    }

    public async Task Stop(CancellationToken cancellationToken = default)
    {
        await httpClient.PutAsync("/startstop/stop", null, cancellationToken);
    }
}