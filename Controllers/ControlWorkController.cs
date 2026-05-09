using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OmgtuPortal.Data;

namespace OmgtuPortal.Controllers;

[ApiController]
[Route("api/control-work")]
[Authorize(Roles = "teacher,admin")]
public class ControlWorkController(AppDbContext db, AppSettings settings) : ControllerBase
{
    /// <summary>
    /// Загрузка контрольной работы. Файл сохраняется на диск, метаданные — в БД.
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        [FromForm] string subjectId,
        [FromForm] string groupId,
        IFormFile file)
    {
        if (file.Length == 0)
            return BadRequest(new { Error = "Файл пустой" });

        var maxBytes = settings.MaxFileSizeMb * 1024 * 1024;
        if (file.Length > maxBytes)
            return BadRequest(new { Error = $"Файл превышает {settings.MaxFileSizeMb} МБ" });

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), settings.UploadsPath);
        Directory.CreateDirectory(uploadsDir);

        var storedName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var storedPath = Path.Combine(uploadsDir, storedName);

        await using (var stream = new FileStream(storedPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? User.FindFirstValue("sub")
                     ?? "unknown";

        var controlWork = new ControlWork
        {
            Id = Guid.NewGuid(),
            SubjectId = subjectId,
            GroupId = groupId,
            FileName = file.FileName,
            StoredPath = storedName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedBy = userId,
            UploadedAt = DateTime.UtcNow,
        };

        db.ControlWorks.Add(controlWork);
        await db.SaveChangesAsync();

        return Created($"/api/control-work/{controlWork.Id}", new
        {
            controlWork.Id,
            controlWork.SubjectId,
            controlWork.GroupId,
            controlWork.FileName,
            controlWork.FileSize,
            controlWork.UploadedAt,
        });
    }

    /// <summary>
    /// Удаление контрольной работы по id.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var file = await db.ControlWorks.FindAsync(id);
        if (file is null)
            return NotFound(new { Error = "Файл не найден" });

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), settings.UploadsPath, file.StoredPath);
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);

        db.ControlWorks.Remove(file);
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Список контрольных работ (опционально по группе).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? groupId)
    {
        var query = db.ControlWorks.AsQueryable();

        if (!string.IsNullOrEmpty(groupId))
            query = query.Where(c => c.GroupId == groupId);

        var list = await query
            .OrderByDescending(c => c.UploadedAt)
            .Select(c => new
            {
                c.Id,
                c.SubjectId,
                c.GroupId,
                c.FileName,
                c.FileSize,
                c.UploadedAt,
            })
            .ToListAsync();

        return Ok(list);
    }
}
