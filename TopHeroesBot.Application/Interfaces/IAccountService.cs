using TopHeroesBot.Application.DTOs;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Interfaces;

public interface IAccountService
{
    Task<AddAccountResult> AddAccountAsync(
     string uid,
     Func<string, Task>? notify = null);

    Task<List<Account>> GetAllAsync();

    Task<bool> DeleteAsync(string uid);
    Task RunDailyAsync(Func<string, Task>? notify = null);
    Task<bool> RunOneDailyAsync(
    string uid,
    Func<string, Task>? notify = null);
    Task RunGoldAsync(Func<string, Task>? notify = null);
    Task<bool> RunOneGoldAsync(
    string uid,
    Func<string, Task>? notify = null);
    Task<int> DeleteAllAsync();
}