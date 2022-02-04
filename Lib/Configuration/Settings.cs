namespace Lib.Configuration;

public class Settings
{
    public string ApiVersion { get; set; } = "api-version=6.0";

    public string? ProjectName { get; set; }

    public string? OrganizationName { get; set; }

    public string? PersonalAccessToken { get; set; }

    public string AzureDevopsUrl => @"https://dev.azure.com";

    public string VariableGroupsUrl => $"https://dev.azure.com/{OrganizationName}/{ProjectName}/_apis/distributedtask/variablegroups?{ApiVersion}";
}