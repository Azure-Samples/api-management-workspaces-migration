# Project - Guidance and tooling for migration to workspaces in Azure API Management

This repo contains tools and guidance to migrate entities such as APIs and products from multiple Azure API Management instances into a single, workspace-based API Management instance. The source instances are referred to as "siloed" instances, meaning they are administered and used by separate teams. The destination instance is referred to as a "federated" instance, because the teams' resources are organized into segregated workspaces in a centrally administered service.   
    
![image](./images/project-detail.png)

Learn more about workspaces:

* [Workspaces in Azure API Management](https://aka.ms/apimdocs/workspaces)
* [Announcing General Availability of Workspaces in Azure API Management - Microsoft Community Hub](https://techcommunity.microsoft.com/t5/azure-integration-services-blog/announcing-general-availability-of-workspaces-in-azure-api/ba-p/4210796)


## Migration overview

The migration process for transitioning from siloed API Management instances to a federated instance involves several steps. More information is in the following sections.

-	Configure [Azure APIOps Toolkit](https://github.com/Azure/APIOps), following its guidance, to extract supported artifacts from each siloed instance
-   Locally organize artifacts extracted by APIOps into a workspace folder structure using scripts provided in this repository
- 	Import artifacts extracted by APIOps into the federated instance by pushing the folder to a federated API Management repo and using the APIOps publisher pipeline
-	Migrate resources not supported by APIOps using scripts provided in this repository
-	For resources that aren't supported by APIOps or the provided scripts, perform manual configurations in the federated API Management instance

See [Migrated entities](#migrated-entities) for details on entities that can be migrated and limitations.


## Migration steps

### Prerequisites

1. [Azure PowerShell](https://learn.microsoft.com/en-us/powershell/azure/install-azure-powershell?view=azps-12.3.0#install)
1. [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli#install)
1. Azure subscription access - Ensure you have an active Azure account with Contributor access to manage API Management instances.
1. One or more source API Management instances, and one destination federated API Management instance. Currently, the destination instance must be in the Premium tier.
1. [Azure APIOps Toolkit](https://azure.github.io/apiops) for API Management - Choose installation for [Azure DevOps](https://azure.github.io/apiops/apiops/3-apimTools/apimtools-azdo-2-3-new.html) or [GitHub](https://azure.github.io/apiops/apiops/3-apimTools/apimtools-github-2-4-new.html). Use the APIOps release [v6.0.2-alpha.1.0.3 or higher](https://github.com/Azure/apiops/releases).

### Install migration scripts

1. Open a terminal, and create a folder for the migration.

    ```
    mkdir apim-silo-to-federated
    cd apim-silo-to-federated 
    ```

3. Clone this migration tools repository.

    ```
    git clone  https://github.com/Azure-Samples/api-management-workspaces-migration
    ```

### Migrate entities using APIOps

See the [APIOps documentation](https://azure.github.io/apiops) for detailed guidance on setting up and running APIOps pipelines for the migration. The following are high-level steps. 

1. Create a repository named `federated-apim-apiops` that's used for the migration.

1. Run the APIOps *extractor pipeline* for each siloed API Management instance. For multiple siloed instances, specify different folder names where you want to extract the artifacts in the pipeline, for example, silo001, silo002, etc. 

1. Clone the `federated-apim-apiops` repository under the root folder `apim-silo-to-federated` that you created previously.

    ```
    git clone <repository_url>   
    ```
    
1. In the `.\api-management-workspaces-migration\create-workspace.ps1` script, review the variables defined for source and destination directories and workspace names. The script assumes two siloed instances and workspaces, but you can modify it for a different number of instances.

    ```
    $srcDir1 = ".\federated-apim-apiops\silo001"
    $srcDir2 = ".\federated-apim-apiops\silo002"
    $destDir = ".\federated-apim-apiops\artifacts-workspace"
    $newWorkspace1 = "<name of the workspace1>"
    $newWorkspace2 = "<name of the workspace2>"
    ```

1. Run the PowerShell script to merge siloed artifacts with a workspace folder structure:  

    ```
    PS> .\api-management-workspaces-migration\create-workspace.ps1  
    ```

1. Go to the `federated-apim-apiops` folder.

    ```
    cd federated-apim-apiops 
    ```

1. Push the `artifacts-workspace` workspace folder to the  `federated-apim-apiops` repository.

    ```
    git push federated-apim-apiops
    ```


1. Run the APIOps *publisher pipeline* from the `federated-apim-apiops` repository to the federated API Management instance, using the contents of the `artifacts-workspace` folder.

### Migrate other entities

This repository contains separate PowerShell scripts to migrate users, groups, and subscriptions from a siloed API Management instance to a federated API Management instance. These entities aren't migrated through APIOps. Other entities, such as developer portal content and identity providers, require manual configuration.

To migrate users, groups, and subscriptions:

1. Get an Azure access token.
 
    ```
    az login
    az account set –subscription <subscriptionId> 
    $ACCESS_TOKEN=(az account get-access-token --resource=https://management.azure.com --query accessToken --output tsv)   
    echo "Access Token: $ACCESS_TOKEN"   
    ```
 

1. In the `create-groups-and-groupusers.ps1`, `create-subscriptions.ps`, and `create-users.ps1` scripts in the `api-management-workspaces-migration` folder, set the variables for the siloed and federated API Management instances and for access tokens:  

    #### Siloed API Management instance 
    
    ```
    $siloAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
    $siloAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName 
    $siloAPIMserviceName = "" # Replace with your actual serviceName
    $siloAPIMapiVersion = "2023-09-01-preview"  
    ```
    
    #### Federated API Management instance 
    
    ```
    $federatedAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
    $federatedAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName
    $federatedAPIMserviceName = "" # Replace with your actual serviceName
    $federatedAPIMapiVersion = "2023-09-01-preview"  
    ```

    #### Access tokens  
    
    ```
    $siloAPIMaccessToken = "" # Replace with your actual token
    $federatedAPIMaccessToken = ""  # Replace with your actual token
    ```

1. Run the PowerShell scripts to migrate the entities.
 
    ```
    PS> cd  .\apim-federated-to-siloed 
    PS> .\api-management-workspaces-migration\create-users.ps1 
    PS> .\api-management-workspaces-migration\create-groups-and-groupusers.ps1 
    PS> .\api-management-workspaces-migration\create-subscriptions.ps1
    ```

1. Run the preceding two steps for each siloed instances.

## Migrated entities

The migration tools process the following entities using the APIOps Toolkit and the PowerShell scripts in this repo:

Entity | Tool |
| --- | --- |
| APIs | APIOps |
|Products | APIOps |
| Backends | APIOps |
| Policy fragments | APIOps |
| Tags | APIOps |
| Policies | APIOps |
| Custom loggers | APIOps |
| Users | PowerShell script |
| Groups | PowerShell script |
| Subscriptions | PowerShell script | 

> [!NOTE] 
> Users are migrated at the service level, not to individual workspaces. Users in the federated instance are shared across workspaces.
> The PowerShell script to migrate users will fail if a user is configured in more than one siloed instance. In such cases, deduplicate the users manually before running the script.  

## Limitations

Certain resources and configurations currently [are not supported in API Management workspaces](https://learn.microsoft.com/azure/api-management/workspaces-overview#gateway-constraints) or aren't migrated by the migration tools and may require manual configuration. These include:

* APIs - schemas                                                                                    
* Certificates for frontend mTLS stored in Azure Key Vault                                      
* Custom domains - optionally configure Application Gateway or Azure FrontDoor for the custom domain                        
* Notifications and notification templates             
* Application Insights
* Network configuration 
* Developer portal content 
* Developer portal identity 

## Support

To report issues or suggest features, please open an issue in this repository. This project is not supported through Microsoft Support.

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact opencode@microsoft.com with any additional questions or comments.

## Trademark

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow Microsoft's Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.
