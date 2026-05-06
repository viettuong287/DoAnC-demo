using Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/audiocontent")]
[AllowAnonymous]
public class AudioContentController : ControllerBase
{
    private readonly AppDbContext _context;

    public AudioContentController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("sync")]
    public async Task<IActionResult> SyncData()
    {
        var locations = await _context.Stalls
            .Include(s => s.StallLocations)
            .Include(s => s.StallMedia)
            .Where(s => s.IsActive)
            .Select(s => new
            {
                ServerId = s.Id.ToString(),
                Name = s.Name,
                Category = "Tour", 
                Description = s.Description,
                ImageUrl = s.StallMedia.Where(m => m.IsActive).OrderBy(m => m.SortOrder).Select(m => m.MediaUrl).FirstOrDefault(),
                Latitude = s.StallLocations.Where(l => l.IsActive).Select(l => (double)l.Latitude).FirstOrDefault(),
                Longitude = s.StallLocations.Where(l => l.IsActive).Select(l => (double)l.Longitude).FirstOrDefault()
            })
            .ToListAsync();

        var languages = await _context.Languages
            .Where(l => l.IsActive)
            .Select(l => new
            {
                ServerId = l.Id.ToString(),
                Code = l.Code,
                Name = l.DisplayName ?? l.Name
            })
            .ToListAsync();

        var scripts = await _context.StallNarrationContents
            .Where(c => c.IsActive && c.Stall.IsActive)
            .Select(c => new
            {
                ServerId = c.Id.ToString(),
                LocationId = c.StallId.ToString(),
                LanguageId = c.LanguageId.ToString(),
                Title = c.Title,
                Content = c.ScriptText
            })
            .ToListAsync();

        return Ok(new
        {
            Locations = locations,
            Languages = languages,
            Scripts = scripts
        });
    }
}
