# Define your subscriptionId, resourceGroupName, and serviceName  for your siloed APIM instance
$siloAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
$siloAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName
$siloAPIMserviceName = "" # Replace with your actual serviceName
$siloAPIMapiVersion = "2023-09-01-preview" 

# Define your subscriptionId, resourceGroupName, and serviceName  for your federated APIM instance
$federatedAPIMsubscriptionId = ""  # Replace with your actual subscriptionId
$federatedAPIMresourceGroupName = ""  # Replace with your actual resourceGroupName
$federatedAPIMserviceName = "" # Replace with your actual serviceName
$federatedAPIMapiVersion = "2023-09-01-preview" 

# Get the access token 
$siloAPIMaccessToken = "" # Replace with your actual token
$federatedAPIMaccessToken = ""  # Replace with your actual token


# Define the URL to fetch the list of users  
$listAllUsers = "https://management.azure.com/subscriptions/$siloAPIMsubscriptionId/resourceGroups/$siloAPIMresourceGroupName/providers/Microsoft.ApiManagement/service/$siloAPIMserviceName/users?api-version=$siloAPIMapiVersion" # Replace with your actual endpoint  

# Invoke the REST API  
$response = Invoke-RestMethod -Method Get -Uri $listAllUsers -Headers @{Authorization = "Bearer $siloAPIMaccessToken" } -ContentType "application/json"  


# Parse the response and loop through each users  
foreach ($user in $response.value) {   
  $userId = $user.id -split '/' | Select-Object -Last 1 
  
  
  # Define the base URL for the API request  
  $baseUrl = "https://management.azure.com/subscriptions/$federatedAPIMsubscriptionId/resourceGroups/$federatedAPIMresourceGroupName/providers/Microsoft.ApiManagement/service/$federatedAPIMserviceName/users/$($userId)?api-version=$federatedAPIMapiVersion"  
  
  
  $body = @{  
    properties = @{  
      firstName = $user.properties.firstName  
      lastName  = $user.properties.lastName
      email     = $user.properties.email
      #   identities = $user.properties.identities  
    }  
  } | ConvertTo-Json 
  
  # Invoke the REST API  
  Invoke-RestMethod -Method Put -Uri $baseUrl -Body $body -Headers @{Authorization = "Bearer $federatedAPIMaccessToken" } -ContentType "application/json"  
} 