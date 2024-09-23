# Project - Migration of Multiple Siloed APIM Instances to a Federated Workspace-Based APIM  

The goal of this project is to support the migration of multiple siloed API Management (APIM) instances into a single, workspace-based federated APIM instance. This capability aims to streamline operations, enhance management efficiency, and reduce complexity for users managing dispersed APIM environments.

![image](./images/project-detail.png)

## Features

Here are some potential features for a project focused on migrating siloed API Management (APIM) instances to a workspace-based federated single APIM instance:

1. Automated Migration Tool: A tool that automates the migration process, ensuring a smooth transition from multiple siloed APIM instances to a unified workspace-based instance.
1. Instance Consolidation: Ability to merge multiple APIM instances into a single, cohesive workspace.
1. Guidance Documentation: Comprehensive documentation and best practices and limitation to assist users throughout the migration process.
1. User Access Management: Centralized management of user roles and permissions within the new workspace-based APIM instance.

## Getting Started

This section provides a concise overview for getting started with the project, ensuring users have the necessary tools and permissions, and guiding them through the initial setup and configuration steps.

### Prerequisites

Install Azure PowerShell
Azure Subscription Access - Ensure you have an active Azure account with sufficient permissions to manage APIM instances.
Install Azure CLI


### Installation

#### Clone the Repository

Clone this repository to your local machine:

```
git clone <repository_url>  
cd <repository_directory>  
```

#### This project depends on existing [APIOps](https://azure.github.io/apiops/apiops/0-labPrerequisites/apim-prereq-0-1.html) tool

 Please check the configuration steps for APIOps.

#### Get Access Token for Azure APIM Management APIs

Run the following command to get an access token:

```azurecli
ACCESS_TOKEN=$(az account get-access-token --resource=https://management.azure.com --query accessToken --output tsv)  
echo "Access Token: $ACCESS_TOKEN"  
```
Replace the access token before executing each script.

### Steps

1. Run APIOps to [Extract Artifacts](https://azure.github.io/apiops/apiops/4-extractApimArtifacts/):
    Use APIOps to extract artifacts from each siloed APIM instance.

1. Merge Artifacts into a Single Folder with Multiple Workspaces
   Execute the [script](create-workspace.ps1) to merge the extracted artifacts into a single folder with multiple workspaces

1. Run APIOps to [Publish Artifacts](https://azure.github.io/apiops/apiops/5-publishApimArtifacts/)
   Use APIOps to publish the merged artifacts to the federated APIM instance


## Entity mapping list
This section outlines the mapping of entities from siloed APIM instances to a workspace-based APIM setup, detailing the migration methods used.

### Migration through APIOps
 
| Siloed APIM              | Workspace-based APIM | Workspace Level          | Migration method (manual / APIOps / FTA scripts) |
| ------------------------ | -------------------- | ------------------------ | ------------------------------------------------ |
| APIs - APIs              | Yes                  | Workspace - APIs         | APIOps                                           |
| APIs - Products          |                      | APIs - Products          | APIOps                                           |
| APIs - Named values      |                      | APIs - Named values      | APIOps                                           |
| APIs - Backends          |                      | APIs - Backends          | APIOps                                           |
| APIs - Policy fragemnts  |                      | APIs - Policy fragemnts  | APIOps                                           |
| APIs- API Tags           |                      | APIs- API Tags           | APIOps                                           |
| APIs - Policy (all APIs) |                      | APIs - Policy (all APIs) | APIOps                                           |
| Custom Loggers           |                      | Supported                | APIOps                                           |


### Use Scripts

| Siloed APIM                     | Workspace-based APIM | Workspace Level                | Migration method (manual / APIOps / FTA scripts) |
| ------------------------------- | -------------------- | ------------------------------ | ------------------------------------------------ |
| Developer Portal - Users        | Yes                  | Developer Portal - Users       | [Powershell Script](create-users.ps1)                             |
| Developer Portal - User Groups  |                      | Developer Portal - User Groups | [Powershell Script](create-groups-and-groupusers.ps1)                             |
| APIs - Subscriptions(with Keys) |                      | APIs - Subscriptions           | [Powershell Script](create-subscriptions.ps1)                             |

### Limitation - Not supported at workspace level at the moment

| Siloed APIM                                        | Workspace-based APIM | Workspace Level                          |
| -------------------------------------------------- | -------------------- | ---------------------------------------- |
| APIs- Credential manager/Authorization             | No                   | Not Supported at workspace at the moment |
| Deployment & Infrastructure - Locations            |                      | Not Supported at workspace at the moment |
| Deployment & Infrastructure -Scale Out             |                      | Not Supported at workspace at the moment |
| Deployment & Infrastructure - self hosted gateways |                      | Not Supported at workspace at the moment |
| Managed identities -User assigned                  |                      | Not Supported at workspace at the moment |
| Managed identities - System Assigned               |                      | Not Supported at workspace at the moment |
| Protocols+Ciphers                                  |                      | Not Supported at workspace at the moment |


### Limitation - Guidance to configure for the federated APIM

| Siloed APIM                            | Workspace-based APIM | Workspace Level                           | Migration method (manual / APIOps / FTA scripts)                                |
| -------------------------------------- | -------------------- | ----------------------------------------- | ------------------------------------------------------------------------------- |
| APIs - Schemas                         | Yes                  | APIs - Schemas                            | Manual configuration                                                            |
| Certificate - cert for frontend mtls   |                      | workspace - supported                     | Manual upload(except Azure Key Vault)                                           |
| D&I  - Custom domains                  |                      | Not supported at workspace at the momenet | Configure AppGatewy/AzureFont Door for the custom domain                        |
| D&I- notifications first 2             |                      | workspace - supported                     | Manually configure the emails, Add in the workspace level in the azure portal   |
| D&I  - Notifications - rest            |                      | Not supported at workspace                | By design                                                                       |
| Application Insights  -API             |                      | workspace - supported                     | Manual configuration                                                            |
| Application Insights - Instance        |                      |                                           | Manual configuration                                                            |
| Deplpyment & infrastructure  - Network |                      | workspace - supported                     | Manual configuration.Cannot reuse the same vnet but same config can be applied. |

### Out of scope items for this tool

| Siloed APIM                        | Workspace-based APIM | Workspace Level              | Migration method (manual / APIOps / FTA scripts) |
| ---------------------------------- | -------------------- | ---------------------------- | ------------------------------------------------ |
| Developer Portal - Portal Overview | No                   | Out of scope - at the moment | Out of scope - at the moment                     |
| Developer Portal - Portal Settings |                      | Out of scope - at the moment | Out of scope - at the moment                     |
| Developer Portal - Identities      |                      | Out of scope - at the moment | Out of scope - at the moment                     |
| Developer Portal - Delegation      |                      | Out of scope - at the moment | Out of scope - at the moment                     |
| Developer Portal - oAuh            |                      | Out of scope - at the moment | Out of scope - at the moment                     |

## Contributing

This project welcomes contributions and suggestions. Most contributions require you to agree to a Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the Microsoft Open Source Code of Conduct. For more information see the Code of Conduct FAQ or contact opencode@microsoft.com with any additional questions or comments.

## Trademark

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow Microsoft's Trademark & Brand Guidelines. Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.