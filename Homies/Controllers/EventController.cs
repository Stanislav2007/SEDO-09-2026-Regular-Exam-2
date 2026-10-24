public async Task<IActionResult> Leave(int id)
{
    string currentUserId = GetUserId();

    var eventToLeave = await _data.Events.FindAsync(id);

    if (eventToLeave == null)
    {
        return NotFound();
    }

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
