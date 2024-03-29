﻿
using MigrationTool.Migration.Domain.Clients;
using MigrationTool.Migration.Domain.Clients.Abstraction;
using MigrationTool.Migration.Domain.Entities;
using MigrationTool.Migration.Domain.Extensions;
using MigrationTool.Migration.Domain.Operations;

namespace MigrationTool.Migration.Domain.Executor.Operations;

public class VersionSetCopyOperationHandler : OperationHandler
{
    private readonly IVersionSetClient VersionSetClient;

    public VersionSetCopyOperationHandler(IVersionSetClient versionSetClient, EntitiesRegistry registry) : base (registry)
    {
        this.VersionSetClient = versionSetClient;
    }

    public override EntityType UsedEntities => EntityType.VersionSet;
    public override Type OperationType => typeof(CopyOperation);

    public override async Task Handle(IMigrationOperation operation, string workspaceId)
    {
        var copyOperation = this.GetOperationOrThrow<CopyOperation>(operation);

        var originalVersionSet = copyOperation.Entity as VersionSetEntity ?? throw new InvalidOperationException();
        var versionSetTemplate = originalVersionSet.ArmTemplate.Copy();
        versionSetTemplate.Name = $"{versionSetTemplate.Name}-in-{workspaceId}";
        versionSetTemplate.Properties.DisplayName = $"{versionSetTemplate.Properties.DisplayName}-in-{workspaceId}";
        
        var newVersionSet = await this.VersionSetClient.Create(versionSetTemplate, workspaceId);
        this.registry.RegisterMapping(originalVersionSet, newVersionSet);
    }
}
