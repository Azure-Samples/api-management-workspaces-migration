# Define source and destination directories and workspace names  
$srcDir1 = ".\federated-apim-apiops\silo001"  
$srcDir2 = ".\federated-apim-apiops\silo002"  
$destDir = ".\federated-apim-apiops\artifacts-workspace"  
$newWorkspace1 = "<name of the workspace1>"
$newWorkspace2 = "<name of the workspace2>"


# Function to copy files and update content  
function Copy-And-UpdateFiles {  
    param (  
        [string]$srcDir,  
        [string]$destWOrkspace  
    )  
# Create destination directory if it doesn't exist  
if (-not (Test-Path -Path $destWOrkspace)) {  
    New-Item -ItemType Directory -Path $destWOrkspace  
} 

 

 # Get all files and directories in the source directory  
 $items = Get-ChildItem -Path $srcDir -Recurse 


 foreach ($item in $items) {  
    # Construct the destination path  
    $destPath = $item.FullName.Replace($srcDir, $destWOrkspace)  

    if ($item.PSIsContainer) {  
        # Create directory if it's a folder  
        if (-not (Test-Path -Path $destPath)) {  
            New-Item -ItemType Directory -Path $destPath  
        }  
    }
    else {  
        # Copy file if it's a file  
        Copy-Item -Path $item.FullName -Destination $destPath  
    }
 }
}      

# Define the full path for the new workspaces in the destination directory  
$destWOrkspace1 = Join-Path -Path $destDir -ChildPath $newWorkspace1 
$destWOrkspace2 = Join-Path -Path $destDir -ChildPath $newWorkspace2 

# Call the function to copy files and update content to the new subfolder  
Copy-And-UpdateFiles -srcDir (Resolve-Path $srcDir1).Path -destWorkspace $destWorkspace1
Copy-And-UpdateFiles -srcDir (Resolve-Path $srcDir2).Path -destWorkspace $destWorkspace2
