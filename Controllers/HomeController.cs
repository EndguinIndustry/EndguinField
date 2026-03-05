using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using EndguinField.Models;
using EndguinField.Models.ViewModels;
using EndguinField.Services;

namespace EndguinField.Controllers;

public class HomeController : Controller
{
    private readonly IUserService _userService;
    private readonly IEventService _eventService;
    private readonly IParticipationService _participantService;


    public HomeController(IUserService userService, IEventService eventService, IParticipationService participantService)
    {
        _userService = userService;
        _eventService = eventService;
        _participantService = participantService;
    }

    public async Task<IActionResult> Index()
    {
        string currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;

        var pendingEventsIds = await _userService.GetAllPendingEventAsync(currentUserId);
        var pendingEventsList = await Task.WhenAll(pendingEventsIds.Select(e => _eventService.GetEventByIdAsync(e)).ToList());

        var pendingEvents = pendingEventsList.Select(e => new EventCardModel
        {
            Id = e.Id,
            Name = e.Name,
            ImgSrc = e.Images[0],
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Location = e.Location,
            ParticipantCount = _participantService.GetAcceptedParticipantCountAsync(e.Id).GetAwaiter().GetResult(),
            MaxParticipants = e.MaxParticipants
        }).ToList();

        var participatingEventsIds = await _userService.GetAllParticipatingEventAsync(currentUserId);
        var participatingEventsList = await Task.WhenAll(participatingEventsIds.Select(e => _eventService.GetEventByIdAsync(e)).ToList());

        var participatingEvents = participatingEventsList.Select(e => new EventCardModel
        {
            Id = e.Id,
            Name = e.Name,
            ImgSrc = e.Images[0],
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Location = e.Location,
            ParticipantCount = _participantService.GetAcceptedParticipantCountAsync(e.Id).GetAwaiter().GetResult(),
            MaxParticipants = e.MaxParticipants
        }).ToList();

        var openEventsList = await _eventService.GetAllEventsAsync();
        var openEvents = openEventsList.Select(e => new EventCardModel
        {
            Id = e.Id,
            Name = e.Name,
            ImgSrc = e.Images[0],
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            Location = e.Location,
            ParticipantCount = _participantService.GetAcceptedParticipantCountAsync(e.Id).GetAwaiter().GetResult(),
            MaxParticipants = e.MaxParticipants
        }).ToList();

        var viewModel = new HomeViewModel
        {
            ParticipatingEvents = participatingEvents,
            PendingEvents = pendingEvents,
            OpenEvents = openEvents
        };
        return View(viewModel);
    }

    public async Task<IActionResult> Privacy()
    {
        string currentUserId = "1";
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    public async Task<IActionResult> Login()
    {
        string currentUserId = "1";
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Error()
    {
        string currentUserId = "1";
        var currentUser = await _userService.GetUserByIdAsync(currentUserId);
        ViewBag.CurrentUser = currentUser;
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
