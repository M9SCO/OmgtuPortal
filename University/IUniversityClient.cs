namespace OmgtuPortal.University;

public interface IUniversityClient
{
    Task<IReadOnlyList<StudentGroup>> GetStudentGroupsAsync();
    Task<string?> GetStudentGroupIdAsync(string sub);
}

public record StudentGroup(string Id, string Name);
