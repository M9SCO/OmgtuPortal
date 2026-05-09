using System.Net.Http.Json;

namespace OmgtuPortal.University;

public class HttpUniversityClient(HttpClient httpClient) : IUniversityClient
{
    public async Task<IReadOnlyList<StudentGroup>> GetStudentGroupsAsync()
    {
        var groups = await httpClient.GetFromJsonAsync<List<StudentGroup>>("groups");
        return groups ?? [];
    }

    public async Task<string?> GetStudentGroupIdAsync(string sub)
    {
        var response = await httpClient.GetFromJsonAsync<GroupIdResponse>($"groups/{sub}");
        return response?.GroupId;
    }

    public async Task<IReadOnlyList<Subject>> GetSubjectsAsync()
    {
        var subjects = await httpClient.GetFromJsonAsync<List<Subject>>("subjects");
        return subjects ?? [];
    }

    private record GroupIdResponse(string? GroupId);
}
