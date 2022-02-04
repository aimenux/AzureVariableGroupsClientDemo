using Lib.Models;

namespace Lib;

public interface IAzureDevopsClient
{
    Task<ICollection<AzureVariableGroup>> GetAzureVariableGroupsAsync(AzureDevopsChoice choice, CancellationToken cancellationToken = default);
}