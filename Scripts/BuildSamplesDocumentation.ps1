$folders = @(".\samples")
$outputFolder = ".\docs\codesamples"

foreach ($rootFolder in $folders) {
    $rootPath = Convert-Path .\

    Get-ChildItem -Path $rootFolder -Recurse -Filter "*sample.cs" | ForEach-Object {
        $filePath = $_.FullName
        $fileName = $_.Name -replace '\.cs$', ''
        $outputPath = Join-Path -Path $outputFolder -ChildPath "$fileName.html"
        $code = Get-Content $filePath -Raw
        $escapedCode = [System.Net.WebUtility]::HtmlEncode($code)

        $outputContent = "<pre><code class=""language-csharp"">$escapedCode</code></pre>"

        $outputContent | Out-File -FilePath $outputPath -Force
    }
}
