using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmgtuPortal.University;

namespace OmgtuPortal.Controllers;

/// <summary>
/// Эндпоинты преподавателя. Доступ: роли teacher и admin.
/// </summary>
[ApiController]
[Route("api/teacher")]
[Authorize(Roles = "teacher,admin")]
public class TeacherController(IUniversityClient universityClient) : ControllerBase
{
    [HttpGet("groups")]
    public async Task<IActionResult> GetGroups()
    {
        return Ok(await universityClient.GetStudentGroupsAsync());
    }
}
