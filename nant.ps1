# Specify the path where you've downloaded the NAnt ZIP file
$NantInstallerPath = "C:\NAnt.zip"

# Specify the installation directory for NAnt
$InstallDirectory = "C:\Program Files\NAnt"

# Extract NAnt to the installation directory
Expand-Archive -Path $NantInstallerPath -DestinationPath $InstallDirectory

# Add NAnt to the system PATH
$env:Path += ";$InstallDirectory\bin"

# Verify installation
Write-Host "NAnt installed successfully!"

# Optional: Clean up the downloaded ZIP file
Remove-Item -Path $NantInstallerPath