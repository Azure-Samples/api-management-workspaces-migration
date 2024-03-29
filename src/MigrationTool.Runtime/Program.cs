using CommandLine;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Commands.Configurations;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Extractor.Models;
using Microsoft.Extensions.DependencyInjection;
using MigrationTool;
using MigrationTool.Migration.Domain;
using MigrationTool.Migration.Domain.Clients;
using MigrationTool.Migration.Domain.Dependencies;
using MigrationTool.Migration.Domain.Entities;
using MigrationTool.Migration.Domain.Executor;
using MigrationTool.Migration.Domain.Planner;
using Sharprompt;
using MigrationTool.Migration.Domain.Operations;
using System.Configuration;
using MigrationTool.Migration.Domain.Dependencies.Resolvers;
using MigrationTool.Migration.Domain.Executor.Operations;
using MigrationTool.Migration.Domain.Clients.Abstraction;

public class Program
{
    public static IServiceProvider ServiceProvider;
    public static WaitSpinner spinner;

    public static async Task Main(string[] args)
    {
        spinner = new WaitSpinner();
        await Parser.Default.ParseArguments<MigrationProgramConfig>(args).WithParsedAsync(MigrationProgram);
        spinner.Dispose();
    }
    public static async Task MigrationProgram(MigrationProgramConfig config)
    {
        ServiceProvider = CreateServiceProvider(config);
        Console.WriteLine(ConfigurationManager.AppSettings["apiFetching"]);
        var apis = await ChooseApis();
        Console.WriteLine(ConfigurationManager.AppSettings["workspaceFetching"]);
        var workspace = await ChooseWorkspace();
        if (workspace == null)
        {
            Console.WriteLine(ConfigurationManager.AppSettings["workspaceMissing"]);
            return;
        }

        var dependencyGraphBuilder = ServiceProvider.GetRequiredService<DependencyGraphBuilder>(); 
        Console.WriteLine(ConfigurationManager.AppSettings["dependenciesFetching"]);

        var graph = await LongRunning(() => dependencyGraphBuilder.Build(apis)); 
        if (graph == null)
        {
            return;
        }
        Console.WriteLine(ConfigurationManager.AppSettings["migrationPlanBuilding"]);
        var plan = MigrationPlanner.Plan(graph, MigrationType.Copy);

        Console.WriteLine(plan); 
        if (Prompt.Confirm(ConfigurationManager.AppSettings["migrationPlanConfirmation"]))
        {
            var executor = ServiceProvider.GetRequiredService<MigrationPlanExecutor>();
            Console.WriteLine(ConfigurationManager.AppSettings["migrationOnGoing"]); 

            await LongRunning(() => executor.Execute(plan, workspace));

            Console.WriteLine(ConfigurationManager.AppSettings["migrationDone"]);
        }
    }

    private static async Task<IEnumerable<Entity>> ChooseApis()
    {
        var apisClient = ServiceProvider.GetRequiredService<IApiClient>();
        var apis = await LongRunning(() => apisClient.FetchAllApisAndVersionSets());
        var selected = Prompt.MultiSelect(ConfigurationManager.AppSettings["apiSelection"], apis);

        HashSet<Entity> versionedApis = new HashSet<Entity>();
        selected.Where(item => item.Type == EntityType.VersionSet).ToList().ForEach(versionSet =>
        {
            versionedApis.UnionWith(((VersionSetEntity)versionSet).Apis);
        });

        List<Entity> allApis = new List<Entity>(); 
        allApis.AddRange(selected.Where(item => item.Type == EntityType.Api));
        allApis.AddRange(versionedApis);
        return allApis;
    }

    private static async Task<string?> ChooseWorkspace()
    {
        var workspaceService = ServiceProvider.GetRequiredService<WorkspaceClient>();
        var workspaces = await LongRunning(() => workspaceService.FetchAll());
        if (workspaces.Count > 0)
            return Prompt.Select(ConfigurationManager.AppSettings["workspaceSelection"], workspaces);
        return null;
    }

    private static async Task LongRunning(Func<Task> task)
    {
        spinner.Start();
        await task();
        spinner.Stop();
    }

    private static async Task<T> LongRunning<T>(Func<Task<T>> task)
    {
        spinner.Start();
        var result = await task();
        spinner.Stop();
        return result;
    }

    private static IServiceProvider CreateServiceProvider(MigrationProgramConfig config)
    {
        IServiceCollection collection = new ServiceCollection();
        ServiceExtensions.AddArmTemplatesServices(collection, null);

        var extractorParamters = new ExtractorParameters(new ExtractorConsoleAppConfiguration()
            { ResourceGroup = config.ResourceGroup, SourceApimName = config.ServiceName });
        collection.AddSingleton(extractorParamters);


        collection.AddSingleton<IApiClient, ApiClient>();
        collection.AddSingleton<NamedValuesClient, NamedValuesClient>();
        collection.AddSingleton<IPolicyFragmentClient, PolicyFragmentClient>();
        collection.AddSingleton<IProductClient, ProductClient>();
        collection.AddSingleton<WorkspaceClient, WorkspaceClient>();
        collection.AddSingleton<ISubscriptionClient, SubscriptionClient>();
        collection.AddSingleton<IVersionSetClient, VersionSetClient>();
        collection.AddSingleton<IGatewayClient, GatewayClient>();
        collection.AddSingleton<ITagClient, TagClient>();
        collection.AddSingleton<IGroupsClient, GroupsClient>();

        collection.AddSingleton<IPolicyRelatedDependenciesResolver, PolicyRelatedDependenciesResolver>();
        collection.AddSingleton<DependencyService, DependencyService>();
        collection.AddSingleton<IEntityDependencyResolver, ApiDependencyResolver>();
        collection.AddSingleton<IEntityDependencyResolver, ProductDependencyResolver>();
        collection.AddSingleton<IEntityDependencyResolver, ApiVersionSetDependencyResolver>();
        collection.AddSingleton<IEntityDependencyResolver, NamedValueDependencyResolver>();
        collection.AddSingleton<IEntityDependencyResolver, TagsDependencyResolver>();
        collection.AddSingleton<IEntityDependencyResolver, ApiOperationDependencyResolver>();
        collection.AddSingleton<ITagsDependencyResolver, TagsDependencyResolver>();
        collection.AddSingleton<IEntityDependencyResolver, GroupDependencyResolver>();
        collection.AddSingleton<IEntityDependencyResolver>(_ => new NoDependencyResolver(EntityType.PolicyFragment));
        collection.AddSingleton<IEntityDependencyResolver>(_ => new NoDependencyResolver(EntityType.Subscription));
        collection.AddSingleton<IEntityDependencyResolver>(_ => new NoDependencyResolver(EntityType.User));

        collection.AddSingleton<DependencyGraphBuilder, DependencyGraphBuilder>();
        collection.AddSingleton<EntitiesRegistry, EntitiesRegistry>();
        collection.AddSingleton<PolicyModifier, PolicyModifier>();
        collection.AddSingleton<MigrationPlanExecutor, MigrationPlanExecutor>();
        collection.AddSingleton<OperationHandler, ApiCopyOperationHandler>();
        collection.AddSingleton<OperationHandler, GroupCopyHandler>();
        collection.AddSingleton<OperationHandler, ProductCopyOperationHandler>();
        collection.AddSingleton<OperationHandler, ProductApiConnectionHandler>();
        collection.AddSingleton<OperationHandler, UserGroupConnectionHandler>();
        collection.AddSingleton<OperationHandler, ProductGroupConnectionHandler>();
        collection.AddSingleton<OperationHandler, SubscriptionCopyHandler>();
        collection.AddSingleton<OperationHandler, VersionSetCopyOperationHandler>();
        collection.AddSingleton<OperationHandler, ProductTagConnectionHandler>();
        collection.AddSingleton<OperationHandler, ApiTagConnectionHandler>();
        collection.AddSingleton<OperationHandler, TagCopyHandler>();
        collection.AddSingleton<OperationHandler, ApiOperationTagConnectionHandler>();
        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.Api | EntityType.Subscription, typeof(ConnectOperation)));
        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.Product | EntityType.Subscription, typeof(ConnectOperation)));

        collection.AddSingleton<OperationHandler, NamedValueCopyHandler>();
        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.Api | EntityType.NamedValue, typeof(ConnectOperation)));

        collection.AddSingleton<OperationHandler, PolicyFragmentsCopyHandler>();
        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.Api | EntityType.PolicyFragment, typeof(ConnectOperation)));

        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.ApiOperation | EntityType.NamedValue, typeof(ConnectOperation)));
        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.Product | EntityType.NamedValue, typeof(ConnectOperation)));
        
        
        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.Api | EntityType.VersionSet, typeof(ConnectOperation)));
        collection.AddSingleton<OperationHandler>(_ =>
            new EmptyHandler(EntityType.Product | EntityType.VersionSet, typeof(ConnectOperation)));

        return collection.BuildServiceProvider();
    }
}