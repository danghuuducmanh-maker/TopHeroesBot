using TopHeroesBot.Application.Enums;
using TopHeroesBot.Application.Interfaces;
using TopHeroesBot.Domain.Entities;

namespace TopHeroesBot.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IAccountService _accountService;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        IAccountService accountService)
    {
        _scheduleRepository = scheduleRepository;
        _accountService = accountService;
    }

    public async Task<ScheduleSetting> GetAsync()
    {
        var setting = await _scheduleRepository.GetAsync();

        if (setting == null)
        {
            setting = new ScheduleSetting
            {
                Hour = 0,
                Minute = 5,
                Enabled = true
            };

            await _scheduleRepository.SaveAsync(setting);
        }

        return setting;
    }

    public async Task SetAsync(int hour, int minute)
    {
        var setting = await GetAsync();

        setting.Hour = hour;
        setting.Minute = minute;

        await _scheduleRepository.SaveAsync(setting);
    }

    public async Task EnableAsync()
    {
        var setting = await GetAsync();

        setting.Enabled = true;

        await _scheduleRepository.SaveAsync(setting);
    }

    public async Task DisableAsync()
    {
        var setting = await GetAsync();

        setting.Enabled = false;

        await _scheduleRepository.SaveAsync(setting);
    }

    public async Task RunNowAsync(
    params RunAction[] actions)
    {
        await _accountService.RunAllAsync(actions);
    }
}