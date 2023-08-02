$rootFolder = ".\samples"
$outputFolder = ".\docfx\codesamples"
$rootPath = Convert-Path .\

Get-ChildItem -Path $rootFolder -Recurse -Filter "*sample.cs" | ForEach-Object {
    $filePath = $_.FullName
    $relativePath = $filePath.Substring($rootPath.Length + 1)
    $relativePath = $relativePath.Replace('\', '/');
    $fileName = $_.Name -replace '\.cs$'  # Remove the ".cs" extension from the file name

    $outputPath = Join-Path -Path $outputFolder -ChildPath "$fileName.md"
    $outputContent = "[!code-csharp[Main](../../$relativePath `"$fileName`")]"

    $outputContent | Out-File -FilePath $outputPath -Force
}

$rootFolder = ".\Tests"
$outputFolder = ".\docfx\codesamples"
$rootPath = Convert-Path .\

Get-ChildItem -Path $rootFolder -Recurse -Filter "*sample.cs" | ForEach-Object {
    $filePath = $_.FullName
    $relativePath = $filePath.Substring($rootPath.Length + 1)
    $relativePath = $relativePath.Replace('\', '/');
    $fileName = $_.Name -replace '\.cs$'  # Remove the ".cs" extension from the file name

    $outputPath = Join-Path -Path $outputFolder -ChildPath "$fileName.md"
    $outputContent = "[!code-csharp[Main](../../$relativePath `"$fileName`")]"

    $outputContent | Out-File -FilePath $outputPath -Force
}