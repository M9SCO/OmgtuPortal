namespace OmgtuPortal.Data;

public class ControlWork
{
    public Guid Id { get; set; }
    public required string SubjectId { get; set; }
    public required string GroupId { get; set; }
    public required string FileName { get; set; }
    public required string StoredPath { get; set; }
    public required string ContentType { get; set; }
    public long FileSize { get; set; }
    public required string UploadedBy { get; set; }
    public DateTime UploadedAt { get; set; }
}
