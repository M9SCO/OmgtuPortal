using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmgtuPortal.Data;
using OmgtuPortal.University;

namespace OmgtuPortal.Controllers;

[ApiController]
[Route("api/contact-work")]
[Authorize(Roles = "student,admin")]
public class StudentFilesController(
    AppDbContext db,
    AppSettings settings,
    IUniversityClient universityClient) : ControllerBase
{
    /// <summary>
    /// Дисциплины, по которым есть файлы для группы студента.
    /// </summary>
    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        var myGroupId = await GetMyGroupId();
        if (myGroupId is null)
            return NotFound(new { Error = "Группа студента не найдена" });

        var subjectIds = await db.ControlWorks
            .Where(c => c.GroupId == myGroupId)
            .Select(c => c.SubjectId)
            .Distinct()
            .ToListAsync();

        var allSubjects = await universityClient.GetSubjectsAsync();
        var result = allSubjects
            .Where(s => subjectIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Список файлов по предмету для группы студента.
    /// </summary>
    [HttpGet("files")]
    public async Task<IActionResult> GetFiles([FromQuery] string subjectId)
    {
        var myGroupId = await GetMyGroupId();
        if (myGroupId is null)
            return NotFound(new { Error = "Группа студента не найдена" });

        var list = await db.ControlWorks
            .Where(c => c.GroupId == myGroupId && c.SubjectId == subjectId)
            .OrderByDescending(c => c.UploadedAt)
            .Select(c => new
            {
                c.Id,
                c.FileName,
                c.FileSize,
                c.ContentType,
                c.UploadedAt,
            })
            .ToListAsync();

        return Ok(list);
    }

    /// <summary>
    /// Скачивание файла по id. Доступен только файл, принадлежащий группе студента.
    /// </summary>
    [HttpGet("download/{fileId:guid}")]
    public async Task<IActionResult> Download(Guid fileId)
    {
        var myGroupId = await GetMyGroupId();
        if (myGroupId is null)
            return NotFound(new { Error = "Группа студента не найдена" });

        var file = await db.ControlWorks.FirstOrDefaultAsync(c => c.Id == fileId);
        if (file is null)
            return NotFound(new { Error = "Файл не найден" });

        if (file.GroupId != myGroupId)
            return Forbid();

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), settings.UploadsPath, file.StoredPath);
        if (!System.IO.File.Exists(fullPath))
            return NotFound(new { Error = "Файл отсутствует на диске" });

        var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        return File(stream, file.ContentType, file.FileName);
    }

    private async Task<string?> GetMyGroupId()
    {
        return await universityClient.GetStudentGroupIdAsync("dev-user-id");
    }
}
