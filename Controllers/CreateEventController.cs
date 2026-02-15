using EndguinField.Services.MongoDB;
using Microsoft.AspNetCore.Mvc;

public class CreateEventController : Controller
{
    private readonly S3Service _s3Service;
    private readonly EventService _eventService;

    public CreateEventController(S3Service s3Service, EventService eventService)
    {
        _s3Service = s3Service;
        _eventService = eventService;
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest("No file uploaded");

        var imageUrl = await _s3Service.UploadFileAsync(image);

        await _eventService.UploadEventImageAsync(imageUrl);
        
        return Ok(new { message = "Upload successful", url = imageUrl });
    }

    public IActionResult Index()
    {
        return View();
    }

}
