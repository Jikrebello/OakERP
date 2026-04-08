namespace OakERP.Client.Configuration;

public sealed class ApiClientOptions
{
    public string BaseUrl { get; set; } = string.Empty;

    public Uri GetBaseUri()
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out Uri? baseUri))
        {
            throw new InvalidOperationException("Api:BaseUrl must be a valid absolute URI.");
        }

        return baseUri;
    }
}
