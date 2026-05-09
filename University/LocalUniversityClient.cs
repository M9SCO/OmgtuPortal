namespace OmgtuPortal.University;

public class LocalUniversityClient : IUniversityClient
{
    private static readonly IReadOnlyList<StudentGroup> Groups =
    [
        new("1", "ИВТ-111"),
        new("2", "ИВТ-112"),
        new("3", "ИВТ-211"),
        new("4", "ПМИ-101"),
        new("5", "ПМИ-201"),
        new("6", "ИСТ-101"),
        new("7", "ИСТ-201"),
        new("8", "МАТ-101"),
    ];

    private static readonly IReadOnlyList<Subject> Subjects =
    [
        new("1", "Математический анализ"),
        new("2", "Линейная алгебра"),
        new("3", "Программирование"),
        new("4", "Базы данных"),
        new("5", "Операционные системы"),
        new("6", "Компьютерные сети"),
    ];

    private static readonly Dictionary<string, string> SubToGroup = new()
    {
        ["dev-user-id"] = "1",
    };

    public Task<IReadOnlyList<StudentGroup>> GetStudentGroupsAsync() =>
        Task.FromResult(Groups);

    public Task<string?> GetStudentGroupIdAsync(string sub)
    {
        SubToGroup.TryGetValue(sub, out var groupId);
        return Task.FromResult(groupId);
    }

    public Task<IReadOnlyList<Subject>> GetSubjectsAsync() =>
        Task.FromResult(Subjects);
}
