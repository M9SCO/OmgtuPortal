namespace OmgtuPortal.University;

public interface IUniversityClient
{
    Task<IReadOnlyList<StudentGroup>> GetStudentGroupsAsync();
    Task<string?> GetStudentGroupIdAsync(string sub);
    Task<IReadOnlyList<Subject>> GetSubjectsAsync();
}

public record StudentGroup(string Id, string Name);
public record Subject(string Id, string Name);
