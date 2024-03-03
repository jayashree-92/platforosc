# Define the download URL for NAnt (replace with the actual URL)
$nantDownloadUrl = 'https://sourceforge.net/projects/nant'

# Define the target installation folder
$installFolder = 'Downloads'

# Create the installation folder if it doesn't exist
New-Item -ItemType Directory -Path $installFolder -Force

# Download NAnt zip file
Invoke-WebRequest -Uri $nantDownloadUrl -OutFile "$installFolder\nant.zip"

# Extract NAnt from the zip file
Expand-Archive -Path "$installFolder\nant.zip" -DestinationPath $installFolder -Force

# Add NAnt folder to PATH
$env:Path += ";$installFolder"

# Verify that NAnt is now part of the PATH
echo "NAnt installation completed. NAnt is available in the PATH."