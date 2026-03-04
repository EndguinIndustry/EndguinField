using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;

namespace EndguinField.Controllers;

public class TagController : Controller
{
    private readonly ITagSchemaServices _tagService;
    private readonly ILogger<TagController> _logger;

    public TagController(ITagSchemaServices tagService, ILogger<TagController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    // Display all tags
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        try
        {
            var tags = await _tagService.GetAllAsync(ct);
            return View(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags");
            ViewBag.Error = "Error retrieving tags: " + ex.Message;
            return View(new List<Tag>());
        }
    }

    // Get tag by ID
    public async Task<IActionResult> Details(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            ViewBag.Error = "Tag ID is required";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var tag = await _tagService.GetByIdAsync(id, ct);
            if (tag == null)
            {
                ViewBag.Error = "Tag not found";
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag with ID {TagId}", id);
            ViewBag.Error = "Error retrieving tag: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // Create tag - GET
    public IActionResult Create()
    {
        return View();
    }

    // Create tag - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("TagName")] Tag tag, CancellationToken ct = default)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var createdTag = await _tagService.CreateAsync(tag, ct);
                TempData["Success"] = "Tag created successfully";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tag");
                ViewBag.Error = "Error creating tag: " + ex.Message;
            }
        }
        return View(tag);
    }

    // Edit tag - GET
    public async Task<IActionResult> Edit(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            ViewBag.Error = "Tag ID is required";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var tag = await _tagService.GetByIdAsync(id, ct);
            if (tag == null)
            {
                ViewBag.Error = "Tag not found";
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag with ID {TagId}", id);
            ViewBag.Error = "Error retrieving tag: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // Edit tag - POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Id,TagName")] Tag tag, CancellationToken ct = default)
    {
        if (id != tag.Id)
        {
            ViewBag.Error = "ID mismatch";
            return View(tag);
        }

        if (ModelState.IsValid)
        {
            try
            {
                var success = await _tagService.UpdateAsync(id, tag, ct);
                if (success)
                {
                    TempData["Success"] = "Tag updated successfully";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewBag.Error = "Failed to update tag";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating tag with ID {TagId}", id);
                ViewBag.Error = "Error updating tag: " + ex.Message;
            }
        }
        return View(tag);
    }

    // Delete tag - GET (confirmation page)
    public async Task<IActionResult> Delete(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            ViewBag.Error = "Tag ID is required";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var tag = await _tagService.GetByIdAsync(id, ct);
            if (tag == null)
            {
                ViewBag.Error = "Tag not found";
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag with ID {TagId}", id);
            ViewBag.Error = "Error retrieving tag: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    // Delete tag - POST
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(id))
        {
            ViewBag.Error = "Tag ID is required";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            var success = await _tagService.DeleteAsync(id, ct);
            if (success)
            {
                TempData["Success"] = "Tag deleted successfully";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.Error = "Failed to delete tag";
                return RedirectToAction(nameof(Index));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag with ID {TagId}", id);
            ViewBag.Error = "Error deleting tag: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
