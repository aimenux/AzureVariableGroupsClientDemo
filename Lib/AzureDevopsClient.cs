using System.Net.Http.Headers;
using System.Text;
using Lib.Configuration;
using Lib.Models;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.DistributedTask.WebApi;

namespace Lib;

public class AzureDevopsClient : IAzureDevopsClient
{
    private readonly HttpClient _httpClient;
    private readonly TaskAgentHttpClient _sdkClient;
    private readonly IOptions<Settings> _options;

    public AzureDevopsClient(HttpClient httpClient, TaskAgentHttpClient sdkClient, IOptions<Settings> options)
    {
        _httpClient = httpClient;
        _sdkClient = sdkClient;
        _options = options;
    }

    public Task<ICollection<AzureVariableGroup>> GetAzureVariableGroupsAsync(AzureDevopsChoice choice, CancellationToken cancellationToken = default)
    {
        return choice switch
        {
            AzureDevopsChoice.Sdk => GetBySdkAsync(cancellationToken),
            AzureDevopsChoice.Rest => GetByRestAsync(cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, $"Unexpected choice {choice}")
        };
    }

    private async Task<ICollection<AzureVariableGroup>> GetBySdkAsync(CancellationToken cancellationToken = default)
    {
        var projectName = _options.Value.ProjectName;
        var results = await _sdkClient.GetVariableGroupsAsync(projectName, cancellationToken: cancellationToken);
        var variableGroups = results.Select(x => new AzureVariableGroup
        {
            Id = x.Id,
            Name = x.Name,
            Variables = x.Variables.ToDictionary(y => y.Key, y => new AzureVariable
            {
                Value = y.Value.Value,
                IsSecret = y.Value.IsSecret,
                IsReadOnly = y.Value.IsReadOnly
            })
        }).ToList();
        return variableGroups;
    }

    public async Task<ICollection<AzureVariableGroup>> GetByRestAsync(CancellationToken cancellationToken = default)
    {
        var requestUrl = _options.Value.VariableGroupsUrl;
        _httpClient.DefaultRequestHeaders.Accept.Add(GetAcceptHeader());
        _httpClient.DefaultRequestHeaders.Authorization = GetAuthorizationHeader();
        using var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsAsync<AzureVariableGroups>(cancellationToken);
        var variableGroups = result.VariableGroups!;
        return variableGroups;
    }

    private static MediaTypeWithQualityHeaderValue GetAcceptHeader() => new MediaTypeWithQualityHeaderValue("application/json");

    private AuthenticationHeaderValue GetAuthorizationHeader()
    {
        var pat = _options.Value.PersonalAccessToken;
        var bytes = Encoding.ASCII.GetBytes($":{pat}");
        var base64 = Convert.ToBase64String(bytes);
        return new AuthenticationHeaderValue("Basic", base64);
    }
}