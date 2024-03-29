﻿using System.Text.RegularExpressions;
using Microsoft.Azure.Management.ApiManagement.ArmTemplates.Common.Templates.NamedValues;
using MigrationTool.Migration.Domain.Entities;

namespace MigrationTool.Migration.Domain.Executor.Operations;

public class PolicyModifier
{
    private static readonly Regex IncludeFragmentFinder =
        new Regex("<include-fragment\\s*fragment-id=\"([^\"]*)\"\\s*/>");
    private static readonly Regex NamedValueFinder = new Regex("{{([^{}]*)}}");

    private readonly EntitiesRegistry registry;

    public PolicyModifier(EntitiesRegistry registry)
    {
        this.registry = registry;
    }

    public string Modify(string policy)
    {
        policy = this.ModifyIncludePolicyFragment(policy);
        policy = this.ModifyNamedValues(policy);
        return policy;
    }

    private string ModifyNamedValues(string policy) =>
        this.ModifyPolicy(policy, IncludeFragmentFinder, EntityType.PolicyFragment);

    private string ModifyIncludePolicyFragment(string policy) =>
        this.ModifyPolicy(policy, NamedValueFinder, EntityType.NamedValue);

    private string ModifyPolicy(string policy, Regex regex, EntityType entityType)
    {
        foreach (var match in regex.Matches(policy).Reverse())
        {
            var matchGroup = match.Groups[1];
            if (this.registry.TryGetMapping(entityType, matchGroup.Value, out var entity))
            {
                string toBeInserted = "";
                switch(entityType)
                {
                    case EntityType.NamedValue:
                        toBeInserted = ((NamedValueTemplateResource)entity.ArmTemplate).Properties.DisplayName; break;
                    case EntityType.PolicyFragment:
                        toBeInserted = entity.DisplayName; break;
                    default:
                        throw new Exception("Provided entity type does not match any known entity that can be present in the policy");
                }
                policy = policy
                    .Remove(matchGroup.Index, matchGroup.Length)
                    .Insert(matchGroup.Index, toBeInserted);
            }
        }

        return policy;
    }
}