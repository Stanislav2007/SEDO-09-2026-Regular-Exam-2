[HttpGet]
public async Task<IActionResult> Add()
{
    var eventModel = new EventFormModel
    {
        Types = GetTypes()
    };

    return await Task.FromResult<IActionResult>(View(eventModel));
}
