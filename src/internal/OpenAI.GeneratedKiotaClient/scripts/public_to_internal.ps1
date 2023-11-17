# Define the path to the script's directory
$scriptPath = $PSScriptRoot

# Define the relative path from the script to the 'generated' directory
$relativePath = "..\generated"

# Combine paths to get the full path to your source code
$sourcePath = Join-Path -Path $scriptPath -ChildPath $relativePath

# Define the file extension to search for
$fileExtension = "*.cs"

# Regex pattern to match public classes, structs, enums, and interfaces
$pattern = '\bpublic\s+(class|struct|enum|interface|record)\s+(\w+)'

# Initialize the counter
$changedFilesCount = 0

# Recursively find all matching files
$files = Get-ChildItem -Path $sourcePath -Filter $fileExtension -Recurse

Write-Host "Found $($files.Count) files to process."
foreach ($file in $files) {
    # Read the contents of the file
    $content = Get-Content $file.FullName -Raw

    # Check if the file contains the pattern
    if ($content -match $pattern) {
        # Replace public with internal
        $updatedContent = [Regex]::Replace($content, $pattern, 'internal $1 $2')

        # Write the updated content back to the file
        Set-Content -Path $file.FullName -Value $updatedContent

        # Increment the counter
        $changedFilesCount++
    }
}

# Print the number of files changed
Write-Host "$changedFilesCount files have been changed."
