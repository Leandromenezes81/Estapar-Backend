using Estapar.ParkingManager.Domain.Attributes;
using Estapar.ParkingManager.Domain.Enums;
using Estapar.ParkingManager.Domain.Exceptions;
using Microsoft.Extensions.Configuration;
using Quartz;
using System.Reflection;

namespace Estapar.ParkingManager.Infrastructure.BackgroundServices.Quartz;

/// <summary>
/// Classe estática para DI do framework Quartz.NET
/// </summary>
public static class QuartzSettingsExtensions
{
    /// <summary>
    /// Método de extensão para configuração genérica de Jobs do Quarts com CRON Expressions no contexto do DI
    /// </summary>
    /// <typeparam name="TJob">Job</typeparam>
    /// <param name="quartzConfigurator">Service Collection do Quartz</param>
    /// <param name="configuration">IConfiguration</param>
    /// <exception cref="NotFoundException">404</exception>
    /// <exception cref="InternalServerErrorException">500</exception>
    public static void AddQuartzJobAndTrigger<TJob>(
        this IServiceCollectionQuartzConfigurator quartzConfigurator,
        IConfiguration configuration)
        where TJob : IJob
    {
        string jobName = typeof(TJob).Name;

        JobAttribute? jobAttribute = typeof(TJob).GetCustomAttribute<JobAttribute>();
        string jobGroup = jobAttribute?.JobGroup.Name ?? string.Empty;
        string jobDescription = jobAttribute?.Description ?? string.Empty;

        string jobConfig = $"{nameof(BackgroundServicesProvider.Quartz)}:{jobGroup}:{jobName}";

        string jobCronExpression = configuration.GetSection(jobConfig).Value ??
            throw new NotFoundException($"Configuração de agendamento de serviço não localizada: {jobDescription}");

        if (!CronExpression.IsValidExpression(jobCronExpression))
        {
            throw new InternalServerErrorException($"A expressão CRON '{jobCronExpression}' do serviço '{jobDescription}' é inválida.");
        }

        JobKey jobKey = new(jobName);

        quartzConfigurator
            .AddJob<TJob>(jobBuilder => jobBuilder.WithIdentity(jobKey));

        quartzConfigurator.AddTrigger(trigger =>
            trigger.ForJob(jobKey)
                .WithIdentity($"{jobName}-trigger")
                .WithCronSchedule(jobCronExpression));

    }

    /// <summary>
    /// Método de extensão para configuração genérica de Jobs do Quarts com Trigger simples no contexto do DI
    /// </summary>
    /// <typeparam name="TJob">Job</typeparam>
    /// <param name="quartzConfigurator">Service Collection do Quartz</param>
    /// <param name="delay">Delay antes da primeira execução</param>
    /// <param name="repeatCount">Quantidade de repetições após a primeira execução; -1 repete indefinidamente.</param>
    /// <param name="interval">Intervalo entre repetições; se omitido, usa o mesmo valor de <paramref name="delay"/>.</param>
    public static void AddQuartzJobWithSimpleTrigger<TJob>(
        this IServiceCollectionQuartzConfigurator quartzConfigurator,
        TimeSpan delay,
        int repeatCount = 0,
        TimeSpan? interval = null)
        where TJob : IJob
    {
        var jobKey = new JobKey(typeof(TJob).Name);
        var repeatInterval = interval ?? delay;

        quartzConfigurator.AddJob<TJob>(opts => opts.WithIdentity(jobKey));

        quartzConfigurator.AddTrigger(opts => opts
            .ForJob(jobKey)
            .WithIdentity($"{jobKey.Name}-simple-trigger")
            .StartAt(DateBuilder.FutureDate((int)delay.TotalSeconds, IntervalUnit.Second))
            .WithSimpleSchedule(x =>
            {
                if (repeatCount == -1)
                    x.RepeatForever().WithInterval(repeatInterval);
                else
                    x.WithRepeatCount(repeatCount).WithInterval(repeatInterval);
            }));
    }
}
