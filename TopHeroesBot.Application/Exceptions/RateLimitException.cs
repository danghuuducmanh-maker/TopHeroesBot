namespace TopHeroesBot.Application.Exceptions;

public class RateLimitException : Exception
{
    public RateLimitException()
        : base("Gift redeem rate limit reached.")
    {
    }
}