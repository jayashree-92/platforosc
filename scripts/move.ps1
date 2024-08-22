# Variables
$sourcePath = "C:\path\to\source\repo"   # Replace with the path to your source repo
$destinationPath = "C:\path\to\destination\repo" # Replace with the path to your destination repo

# Copy all files and folders from the source to the destination
Write-Host "Copying files from $sourcePath to $destinationPath ..."
Copy-Item -Path "$sourcePath\*" -Destination $destinationPath -Recurse -Force

# Verify that the copy was successful
if ($?) {
    Write-Host "Files copied successfully."

    # Optionally, delete the source files after copying
    # Remove-Item -Path "$sourcePath\*" -Recurse -Force
    # Write-Host "Source files deleted."
} else {
    Write-Host "Error during file copy."
}
