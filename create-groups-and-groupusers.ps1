# Define your subscriptionId, resourceGroupName, and serviceName for your siloed APIM instance 
$siloAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
$siloAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName
$siloAPIMserviceName = "" # Replace with your actual serviceName
$siloAPIMapiVersion = "2023-09-01-preview" 

# Define your subscriptionId, resourceGroupName, and serviceName for your federated APIM instance
$federatedAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
$federatedAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName
$federatedAPIMserviceName = "" # Replace with your actual serviceName
$federatedAPIMworkspaceId = ""  # Replace with your actual workspaceId
$federatedAPIMapiVersion = "2023-09-01-preview" 

# Get the access token 
$siloAPIMaccessToken = "" # Replace with your actual token
$federatedAPIMaccessToken = ""  # Replace with your actual token


# Define the URL to fetch the list of groups  
$listAllGroups = "https://management.azure.com/subscriptions/$siloAPIMsubscriptionId/resourceGroups/$siloAPIMresourceGroupName/providers/Microsoft.ApiManagement/service/$siloAPIMserviceName/groups?`$filter=name ne 'administrators' and name ne 'developers' and name ne 'guests'&api-version=$siloAPIMapiVersion" # Replace with your actual endpoint  

# Invoke the REST API  
$response = Invoke-RestMethod -Method Get -Uri $listAllGroups -Headers @{Authorization = "Bearer $siloAPIMaccessToken" } -ContentType "application/json"  


# Parse the response and loop through each users  
foreach ($group in $response.value) {   
  $groupId = $group.id -split '/' | Select-Object -Last 1 
  
  
  # Define the base URL for the API request  
  $baseUrl = "https://management.azure.com/subscriptions/$federatedAPIMsubscriptionId/resourceGroups/$federatedAPIMresourceGroupName/providers/Microsoft.ApiManagement/service/$federatedAPIMserviceName/workspaces/$federatedAPIMworkspaceId/groups/$($groupId)?api-version=$federatedAPIMapiVersion"  
  
  
  $body = @{  
    properties = @{  
      displayName = $group.properties.displayName  
      description = $group.properties.description
      type        = $group.properties.type
      #   identities = $user.properties.identities  
    }  
  } | ConvertTo-Json 
  
  # Invoke the REST API  
  Invoke-RestMethod -Method Put -Uri $baseUrl -Body $body -Headers @{Authorization = "Bearer $federatedAPIMaccessToken" } -ContentType "application/json"
  
  #Get the list of users for each groups
  $listofGroupUsers = "https://management.azure.com/subscriptions/$siloAPIMsubscriptionId/resourceGroups/$siloAPIMresourceGroupName/providers/Microsoft.ApiManagement/service/$siloAPIMserviceName/groups/$($groupId)/users?api-version=$siloAPIMapiVersion"
  
  # Invoke the REST API  
  $groupUsers = Invoke-RestMethod -Method Get -Uri $listofGroupUsers -Headers @{Authorization = "Bearer $siloAPIMaccessToken" } -ContentType "application/json"  
  
  foreach ($groupuser in $groupUsers.value) {
    $userId = $groupuser.id -split '/' | Select-Object -Last 1 
    #Define the URL for the API request  
    $groupCreateUrl = "https://management.azure.com/subscriptions/$federatedAPIMsubscriptionId/resourceGroups/$federatedAPIMresourceGroupName/providers/Microsoft.ApiManagement/service/$federatedAPIMserviceName/workspaces/$federatedAPIMworkspaceId/groups/$($groupId)/users/$($userId)?api-version=$federatedAPIMapiVersion"  
    
    # Invoke the REST API  
    Invoke-RestMethod -Method Put -Uri $groupCreateUrl -Headers @{Authorization = "Bearer $federatedAPIMaccessToken" } -ContentType "application/json"
    
  }
} 