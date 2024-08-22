# Define the source and destination paths
$sourcePath = "\\vmwclnwosdusc01\devops-share"
$destinationRootPath = "C:\Backup"

# Get the current date and time to create a unique folder name
$dateTime = Get-Date -Format "yyyyMMdd-HHmmss"
$destinationFolderPath = Join-Path -Path $destinationRootPath -ChildPath "Backup_$dateTime"

# Create the new destination folder
New-Item -Path $destinationFolderPath -ItemType Directory

# Move all files and directories from the source path to the new destination folder
Get-ChildItem -Path $sourcePath | ForEach-Object {
    Move-Item -Path $_.FullName -Destination $destinationFolderPath -Force
}

# Optional: Confirm the operation is completed
Write-Host "All files and folders have been moved to $destinationFolderPath"
