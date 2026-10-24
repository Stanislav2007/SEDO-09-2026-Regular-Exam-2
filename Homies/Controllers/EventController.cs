
using Homies.Data;
using Homies.Data.Models;
using Homies.Models.Event;
using Homies.Models.Type;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Homies.Controllers
{
    [Authorize]
    public class EventController : Controller
    {
        private readonly HomiesDbContext _data;

        public EventController(HomiesDbContext data)
        {
            _data = data;
        }

        public async Task<IActionResult> Add()
        {
            var eventModel = new EventFormModel
            {
                Types = GetTypes()
            };

            return View(eventModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(EventFormModel eventModel)
        {
            if (!await _data.Types.AnyAsync(t => t.Id == eventModel.TypeId))
            {
                ModelState.AddModelError(
                    nameof(eventModel.TypeId),
                    "Type does not exist!");
            }

            if (!ModelState.IsValid)
            {
                eventModel.Types = GetTypes();
                return View(eventModel);
            }

            var eventToAdd = new Event
            {
                Name = eventModel.Name,
                Description = eventModel.Description,
                CreatedOn = DateTime.Now,
                TypeId = eventModel.TypeId,
                OrganiserId = GetUserId(),
                Start = eventModel.Start,
                End = eventModel.End
            };

            await _data.Events.AddAsync(eventToAdd);
            await _data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        public async Task<IActionResult> All()
        {
            var eventsToDisplay = await _data.Events
                .Select(e => new EventViewShortModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Start = e.Start.ToString("dd/MM/yyyy H:mm"),
                    Type = e.Type.Name,
                    Organiser = e.Organiser.UserName
                })
                .ToListAsync();

            return View(eventsToDisplay);
        }

        public async Task<IActionResult> Join(int id)
        {
            var eventToJoin = await _data.Events.FindAsync(id);

            if (eventToJoin == null)
            {
                return NotFound();
            }

            var currentUserId = GetUserId();

            var alreadyJoined = await _data.EventsParticipants
                .AnyAsync(ep =>
                    ep.EventId == id &&
                    ep.HelperId == currentUserId);

            if (alreadyJoined)
            {
                return RedirectToAction(nameof(Joined));
            }

            var entry = new EventParticipant
            {
                EventId = id,
                HelperId = currentUserId
            };

            await _data.EventsParticipants.AddAsync(entry);
            await _data.SaveChangesAsync();

            return RedirectToAction(nameof(Joined));
        }

        public async Task<IActionResult> Leave(int id)
        {
            var eventToLeave = await _data.Events.FindAsync(id);

            if (eventToLeave == null)
            {
                return NotFound();
            }

            var currentUserId = GetUserId();

            var entry = await _data.EventsParticipants
                .FirstOrDefaultAsync(ep =>
                    ep.HelperId == currentUserId &&
                    ep.EventId == id);

            if (entry == null)
            {
                return NotFound();
            }

            _data.EventsParticipants.Remove(entry);
            await _data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        public async Task<IActionResult> Joined()
        {
            var currentUserId = GetUserId();

            var userEvents = await _data.EventsParticipants
                .Where(ep => ep.HelperId == currentUserId)
                .Select(ep => new EventViewShortModel
                {
                    Id = ep.Event.Id,
                    Name = ep.Event.Name,
                    Start = ep.Event.Start.ToString("dd/MM/yyyy H:mm"),
                    Type = ep.Event.Type.Name
                })
                .ToListAsync();

            return View(userEvents);
        }

        public async Task<IActionResult> Details(int id)
        {
            var eventToDisplay = await _data.Events
                .Where(e => e.Id == id)
                .Select(e => new EventViewDetailsModel
                {
                    Id = e.Id,
                    Name = e.Name,
                    Start = e.Start.ToString("dd/MM/yyyy H:mm"),
                    End = e.End.ToString("dd/MM/yyyy H:mm"),
                    Organiser = e.Organiser.UserName,
                    Type = e.Type.Name,
                    Description = e.Description,
                    CreatedOn = e.CreatedOn.ToString("dd/MM/yyyy H:mm")
                })
                .FirstOrDefaultAsync();

            if (eventToDisplay == null)
            {
                return NotFound();
            }

            return View(eventToDisplay);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var eventToEdit = await _data.Events.FindAsync(id);

            if (eventToEdit == null)
            {
                return NotFound();
            }

            var currentUserId = GetUserId();

            if (currentUserId != eventToEdit.OrganiserId)
            {
                return Forbid();
            }

            var eventModel = new EventFormModel
            {
                Name = eventToEdit.Name,
                Description = eventToEdit.Description,
                Start = eventToEdit.Start,
                End = eventToEdit.End,
                TypeId = eventToEdit.TypeId,
                Types = GetTypes()
            };

            return View(eventModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EventFormModel model)
        {
            var eventToEdit = await _data.Events.FindAsync(id);

            if (eventToEdit == null)
            {
                return NotFound();
            }

            var currentUserId = GetUserId();

            if (currentUserId != eventToEdit.OrganiserId)
            {
                return Forbid();
            }

            if (!await _data.Types.AnyAsync(t => t.Id == model.TypeId))
            {
                ModelState.AddModelError(
                    nameof(model.TypeId),
                    "Type does not exist!");
            }

            if (!ModelState.IsValid)
            {
                model.Types = GetTypes();
                return View(model);
            }

            eventToEdit.Name = model.Name;
            eventToEdit.Description = model.Description;
            eventToEdit.Start = model.Start;
            eventToEdit.End = model.End;
            eventToEdit.TypeId = model.TypeId;

            await _data.SaveChangesAsync();

            return RedirectToAction(nameof(All));
        }

        private IEnumerable<TypeViewModel> GetTypes()
        {
            return _data.Types
                .Select(t => new TypeViewModel
                {
                    Id = t.Id,
                    Name = t.Name
                })
                .ToList();
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? string.Empty;
        }
    }
}

