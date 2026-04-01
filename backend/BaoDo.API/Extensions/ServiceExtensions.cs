using BaoDo.Core.Interfaces;
using BaoDo.Infrastructure.Services;

namespace BaoDo.API.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVocabularyService, VocabularyService>();
        services.AddScoped<IGrammarService, GrammarService>();
        services.AddScoped<IDictationService, DictationService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IDictionaryService, DictionaryService>();
        services.AddScoped<IAICoachService, AICoachService>();

        return services;
    }
}
