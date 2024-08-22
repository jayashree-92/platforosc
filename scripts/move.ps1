Expand-Archive -Path "C:\azure-devops-deployment\HM.Operations.Secure.Web.zip" -DestinationPath "C:\azure-devops-deployment -Force
Copy-Item -Path "C:\azure-devops-deployment\Content\C_C\agent-ic-admin-dev\_work\2\s\HM.Operations.Secure.Web\obj\Local\Package\PackageTmp" -Destination "C:\azure-devops-deployment\deployment" -Recurse -Force



# Variables
$sourcePath = "C:\azure-devops-deployment\deployment"   # Replace with the path to your source repo
$destinationPath = "\\vmwclnwosdusc01\devops-share" # Replace with the path to your destination repo

# Copy all files and folders from the source to the destination
Write-Host "Copying files from $sourcePath to $destinationPath ..."
Copy-Item -Path "$sourcePath\*" -Destination $destinationPath -Recurse -Force

# Verify that the copy was successfu