using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OmgtuPortal.University;

namespace OmgtuPortal.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupsController(IUniversityClient universityClient) : ControllerBase
{
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMy()
    {
        var sub = User.FindFirst("sub")?.Value;
        if (sub is null)
            return Unauthorized();

        var groupId = await universityClient.GetStudentGroupIdAsync(sub);
        return Ok(new { groupId });
    }
}
