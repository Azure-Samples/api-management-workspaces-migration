# Project - Migration of Multiple Siloed APIM Instances to a Federated Workspace-Based APIM  

The goal of this project is to support the migration of multiple siloed API Management (APIM) instances into a single, workspace-based federated APIM instance. This repository contains tools and guidance to execute the migration.This capability aims to streamline operations, enhance management efficiency, and reduce complexity for users managing dispersed APIM environments. There are few limitations listed at the bottom.

![image](./images/project-detail.png)

## Workspace Overview 

Workspaces in Azure API Management introduce a new level of autonomy for an organization's API teams, allowing them to create, manage, and publish APIs more efficiently and securely. These workspaces provide isolated administrative access and API runtime, empowering API teams while enabling the central API platform team to maintain oversight through centralized monitoring, API policy enforcement, compliance, and unified API discovery via a developer portal. Functioning like "folders," each workspace contains APIs, products, subscriptions, named values, and related resources, with access managed through Azure's role-based access control (RBAC). Additionally, each workspace is linked to a workspace gateway that routes API traffic to backend services. 

## Benefits to migrate to federated APIM 

Discover the benefits of federated workspaces in Azure API Management and how they enhance autonomy and efficiency for your API teams by reading more on the Microsoft Tech Community blog [Announcing General Availability of Workspaces in Azure API Management - Microsoft Community Hub](https://techcommunity.microsoft.com/t5/azure-integration-services-blog/announcing-general-availability-of-workspaces-in-azure-api/ba-p/4210796)


## Getting Started

This section provides a concise overview for getting started with the project, ensuring users have the necessary tools and permissions, and guiding them through the initial setup and configuration steps.

### Prerequisites

1. [Install Azure PowerShell](https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell?view=azps-12.3.0#install)
1. [Install WSL](https://learn.microsoft.com/en-us/windows/wsl/install#install-wsl-command)
1. [Install Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli#install)
1. Azure Subscription Access - Ensure you have an active Azure account with contributor access to manage APIM instances.


### Installation

The migration tool for transitioning from siloed API Management (APIM) instances to a federated APIM instance involves several steps.The setup includes configuring APIOps for the federated instance and setting up extractor pipelines for each siloed instance. Artifacts from each siloed instance are extracted and organized into a workspace folder structure, which is then pushed to the federated APIM repository.Then, users run scripts to migrate entities and complete any necessary manual configurations. It's important to note that there are limitations listed below for this migration.


1. Open WSL

2. Create a folder for the migration activity.

```azurecli
mkdir apim-silo-to-federated
cd apim-silo-to-federated 
```

3. Clone the tools repository

```azurecli
git clone  https://github.com/Azure-Samples/api-management-workspaces-migration
```

#### Setup APIOps for the federated instance

4. Following the gudiance in APIOPS doc,,create a repository `federated-apim-apiops` to enable APIOPs for the migration to federated apim instance from siloed apim instance

5. Setup APIOps for the migration thorugh APIOps

 Configure APIM tools for [Azure DevOps](https://azure.github.io/apiops/apiops/3-apimTools/apimtools-azdo-2-3-new.html) or [GitHub](https://azure.github.io/apiops/apiops/3-apimTools/apimtools-github-2-4-new.html). Use the APIOps release[v6.0.2-alpha.1.0.3 or higher ](https://github.com/Azure/apiops/releases)

6. Setup Extractor pipeline for the each siloed instance using Azure DevOps or GitHub


7. Run the extractor pipeline for each Siloed instances (for multiple siloed instances, run the extractor pipeline multiple times to get the artifacts in different folders.
   Mention the folder name where you want to extract the artifacts in the pipeline e.g. silo001 , silo002 etc. 

8. Clone the `federated-apim-apiops` repository under the root folder `apim-silo-to-federated`

```azurecli
git clone <repository_url>   
```

9. Change the variables in .\ api-management-workspaces-migration\create-workspace.ps1 script 

#### Define source and destination directories and workspace names   

```azurepowershell
$srcDir1 = ".\federated-apim-apiops\silo001"
$srcDir2 = ".\federated-apim-apiops\silo002"
$destDir = ".\federated-apim-apiops\artifacts-workspace"
$newWorkspace1 = "<name of the workspace1>"
$newWorkspace2 = "<name of the workspace2>"
```

10. Run the shell scripts to merge siloed artifacts with a workspace folder structure -   -  

```azurepowershell
PS> .\api-management-workspaces-migration \create-workspace.ps1  
```

11. Go to the  <<federated-apim-apiops>>  folder

```azurecli
cd federated-apim-apiops 
```

12. Push the `artifacts-workspace` workspace folder to  `federated-apim-apiops` repository

```azurecli
git push << federated-apim-apiops >>
```

13. Setup the APIOps publisher pipeline from `federated-apim-apiops` repository targets to `atrifacts-workspace` folder.

14. Get the access token

 
```azurecli
az login
az account set –subscription <subscriptionId> 
$ACCESS_TOKEN=(az account get-access-token --resource=https://management.azure.com --query accessToken --output tsv)   
echo "Access Token: $ACCESS_TOKEN"   
```
 

 
15. Change the variables in the scripts in 'api-management-workspaces-migration' folder -  

#### Define your subscriptionId, resourceGroupName, and serviceName  for your siloed APIM instance 

```azurepowershell
$siloAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
$siloAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName 
$siloAPIMserviceName = "" # Replace with your actual serviceName
$siloAPIMapiVersion = "2023-09-01-preview"  
```
  

#### Define your subscriptionId, resourceGroupName, and serviceName  for your federated APIM instance 

```azurepowershell
$federatedAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
$federatedAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName
$federatedAPIMserviceName = "" # Replace with your actual serviceName
$federatedAPIMapiVersion = "2023-09-01-preview"  
```
  

#### Replace the access token  

```azurepowershell
$siloAPIMaccessToken = "" # Replace with your actual token
$federatedAPIMaccessToken = ""  # Replace with your actual token
```

16. Run the shell scripts to migrate the entities through scripts.
 
```azurepowershell
PS> cd  .\apim-federrated-to-siloed 
PS> .\api-management-workspaces-migration\create-users.ps1 
PS> .\api-management-workspaces-migration\create-groups-and-groupusers.ps1 
PS> .\api-management-workspaces-migration\create-subscriptions.ps1
```

17. Run step #15 and #16 for each siloed instances.

18. Complete the manual configuration on federated instances outlined above table.


 
### Entities Migration through APIOps

Below are the entities migrate in workspace level from the siloed APIM through APIOps-

* APIs
* Products
* Backend
* Policy fragments
* Tags
* Policy
* Custom Logger

### Entities Migration through Scripts

* Execute the [Powershell Script](create-users.ps1) to migrate the Users from siloed APIM service to federated APIM service level. Please note that the script will not work if there is a conflict(same user in two different siloed APIM instances) .
* Execute the [Powershell Script](create-groups-and-groupusers.ps1) to migrate the User Groups from siloed APIM service to federated workspace level.
* Execute the [Powershell Script](create-subscriptions.ps1) to migrate the APIs - Subscriptions(with Keys) from siloed APIM service to federated workspace level.

## Current Limitation  

### Limitation - Not supported at workspace level

#### Temporary workspace limitation

Before migrating to Workspaces, please verify that all necessary features are supported. For detailed information, refer to the [documentation](https://aka.ms/apimdocs/workspaces).If customers need this functionality, they should not migrate.

#### Limitation by design

In the process of migrating to Workspaces, it's important to note that some elements may not be supported by the migration tool and will need to be configured manually.Components such as developer portal content and identity providers fall into this category. These elements will reside outside of Workspaces in the destination service, requiring manual configuration to ensure proper functionality.

### Limitation - Guidance to configure for the federated APIM

Below items are currently unsupported by the tool and need to configured manually - 

| Siloed APIM                            | Migration method (manual / APIOps / FTA scripts)                                |
| -------------------------------------- | ------------------------------------------------------------------------------- |
| APIs - Schemas                         | Require manual configuration (not automatically supported by the tool)                                                           |
| Certificate - cert for frontend mtls   | Require manual configuration (not automatically supported by the tool) Azure Key Vault is not supported for Certificate store.)                                           |
| D&I  - Custom domains                  | Require manual configuration (not automatically supported by the tool). Configure AppGatewy/AzureFont Door for the custom domain                        |
| D&I- notifications first 2             | Require manual configuration (not automatically supported by the tool)   |
| Application Insights  -API             | Require manual configuration (not automatically supported by the tool)                                                               |
| Application Insights - Instance        | Require manual configuration (not automatically supported by the tool)                                                               |
| Deployment & infrastructure  - Network | Require manual configuration (not automatically supported by the tool) Cannot reuse the same vnet but same config can be applied. |
| Developer Portal Content | Require manual configuration (not automatically supported by the tool) |
| Developer Portal Identity | Require manual configuration (not automatically supported by the tool) |


## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact opencode@microsoft.com with any additional questions or comments.

## Trademark

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow Microsoft's Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.