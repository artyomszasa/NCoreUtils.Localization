using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace NCoreUtils.Localization;

public class LocalizationSourceRepositoryBuilder(IServiceCollection services)
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private Type? Repository { get; set; }

    public IServiceCollection Services { get; } = services;

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    internal Type GetRepositoryType()
        => Repository is null
            ? throw new InvalidOperationException("No localization source repository has been registered.")
            : Repository;

    public LocalizationSourceRepositoryBuilder AddRepository<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TRepository>()
        where TRepository : class, ILocalizationSourceRepository
    {
        if (Repository is null)
        {
            Repository = typeof(TRepository);
            return this;
        }
        throw new NotSupportedException("Composite repositories not yet supported.");
    }
}