$folders = @(".\samples")
$outputFolder = ".\Samples\Mapsui.Samples.Blazor\wwwroot\codesamples"

foreach ($rootFolder in $folders) {
    $rootPath = Convert-Path .\

    Get-ChildItem -Path $rootFolder -Recurse -Filter "*sample.cs" | ForEach-Object {
        $filePath = $_.FullName
        $fileName = $_.Name -replace '\.cs$', ''
        $outputPath = Join-Path -Path $outputFolder -ChildPath "$fileName.html"
        $code = Get-Content $filePath -Raw
        
        # Note: We don't HTML encode the code since we want it to display properly
        # The browser will handle the content correctly within the <code> tags
        
        $outputContent = @"
<!DOCTYPE html>
<html>
<head>
    <link href="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/themes/prism.min.css" rel="stylesheet" />
    <link href="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.css" rel="stylesheet" />
    <style>
        body { margin: 0; padding: 20px; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif; }
        pre[class*="language-"] { margin: 0; border-radius: 8px; }
    </style>
</head>
<body>
<pre class="line-numbers"><code class="language-csharp">$code</code></pre>

<script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/components/prism-core.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/autoloader/prism-autoloader.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/plugins/line-numbers/prism-line-numbers.min.js"></script>
<script src="https://cdnjs.cloudflare.com/ajax/libs/prism/1.29.0/components/prism-csharp.min.js"></script>
<script>
    document.addEventListener('DOMContentLoaded', function() {
        Prism.highlightAll();
        // Notify parent window that highlighting is complete
        if (window.parent && window.parent !== window) {
            window.parent.postMessage('prism-highlight', '*');
        }
    });
</script>
</body>
</html>
"@

        $outputContent | Out-File -FilePath $outputPath -Encoding UTF8 -Force
    }
}
