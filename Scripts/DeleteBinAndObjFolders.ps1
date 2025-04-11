# Get the root folder and print it
$rootFolder = Get-Location
Write-Host "Root Folder: $rootFolder"

# Get and delete the bin and obj folders while avoiding node_modules
Get-ChildItem .\ -Include bin,obj -Recurse | Where-Object { $_.FullName -notmatch '\\node_modules\\' } | ForEach-Object {
    $folder = $_.FullName
    Write-Host "Attempting to delete: $folder"
    
    try {
        Remove-Item -Path $folder -Force -Recurse -ErrorAction Stop
        # Check if the folder still exists
        if (Test-Path $folder) {
            Write-Host "Failed to delete: $folder (possibly locked)" -ForegroundColor Red
        } else {
            Write-Host "Successfully deleted: $folder" -ForegroundColor Green
        }
    } catch {
        Write-Host "Error deleting: $folder - $_" -ForegroundColor Red
    }
}
