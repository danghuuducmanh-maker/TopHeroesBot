using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Application.Enums;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IAccountService
{
    Task<AddAccountResult> AddAccountAsync(
     string uid,
     Func<string, Task>? notify = null);
    Task<bool> RunAsync(
    string uid,
    RunAction[] actions,
    Func<string, Task>? notify = null);

    Task RunAllAsync(
        RunAction[] actions,
        Func<string, Task>? notify = null);

    Task<List<Account>> GetAllAsync();

    Task<bool> DeleteAsync(string uid);
    
    Task<int> DeleteAllAsync();
}