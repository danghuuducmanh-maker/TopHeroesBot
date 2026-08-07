using TopHeroesBot.Application.Enums;

namespace TopHeroesBot.Application.Helpers;

public static class RunActionHelper
{
    public static RunAction[] Build(
        RunAction action1,
        RunAction? action2 = null,
        RunAction? action3 = null)
    {
        var actions = new List<RunAction>
        {
            action1
        };

        if (action2.HasValue)
            actions.Add(action2.Value);

        if (action3.HasValue)
            actions.Add(action3.Value);

        return actions
            .Distinct()
            .ToArray();
    }
}