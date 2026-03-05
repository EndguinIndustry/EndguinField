using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;

namespace EndguinField.Controllers;

public class CommentController : Controller
{
    private readonly ICommentSchemaServices _commentService;
    private readonly ILogger<CommentController> _logger;

    public CommentController(ICommentSchemaServices commentService, ILogger<CommentController> logger)
    {
        _commentService = commentService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        try
        {
            var items = await _commentService.GetAllAsync(ct);
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comments");
            ViewBag.Error = ex.Message;
            return View(new List<Comment>());
        }
    }

    public async Task<IActionResult> Details(string id, CancellationToken ct = default)
    {
        try
        {
            var item = await _commentService.GetByIdAsync(id, ct);
            return View(item);
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException)
        {
            return RedirectToAction(nameof(Index));
        }
    }

    public IActionResult Create()
    {
        return View(new Comment());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Content,SenderId,RepliedMessageId")] Comment comment, CancellationToken ct = default)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var created = await _commentService.CreateAsync(comment, ct);
                TempData["Success"] = "Comment created";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment");
                ViewBag.Error = ex.Message;
            }
        }
        return View(comment);
    }

    public async Task<IActionResult> Edit(string id, CancellationToken ct = default)
    {
        try
        {
            var item = await _commentService.GetByIdAsync(id, ct);
            return View(item);
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException)
        {
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Id,Content,SenderId,RepliedMessageId,TimeStamp")] Comment comment, CancellationToken ct = default)
    {
        if (id != comment.Id)
        {
            ViewBag.Error = "ID mismatch";
            return View(comment);
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _commentService.UpdateAsync(id, comment, ct);
                TempData["Success"] = "Comment updated";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment");
                ViewBag.Error = ex.Message;
            }
        }
        return View(comment);
    }

    public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
    {
        try
        {
            var item = await _commentService.GetByIdAsync(id, ct);
            return View(item);
        }
        catch (Exception ex) when (ex is KeyNotFoundException || ex is ArgumentException)
        {
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id, CancellationToken ct = default)
    {
        try
        {
            await _commentService.DeleteAsync(id, ct);
            TempData["Success"] = "Comment deleted";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment");
            TempData["Error"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    // Debug endpoint to return raw JSON list of comments
    [HttpGet]
    public async Task<IActionResult> AllJson(CancellationToken ct = default)
    {
        try
        {
            var items = await _commentService.GetAllAsync(ct);
            return Json(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AllJson");
            return StatusCode(500, ex.Message);
        }
    }
}
