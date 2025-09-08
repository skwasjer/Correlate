$csprojFiles = @(
    "../../../src/Correlate.AspNet/Correlate.AspNet.csproj",
    "../../../src/Correlate.Abstractions/Correlate.Abstractions.csproj",
    "../../../src/Correlate.Core/Correlate.Core.csproj"
    "../../../src/Correlate.DependencyInjection/Correlate.DependencyInjection.csproj"
)

# Clean up previous packages
Remove-Item *.nupkg, *.snupkg -ErrorAction SilentlyContinue

# Generate timestamp-based version (YYYY.MM.DD.HHmm)
$buildVersion = (Get-Date).ToString("yyyy.MM.dd.HHmm")

foreach ($csproj in $csprojFiles) {
    if (-not (Test-Path $csproj)) {
        Write-Host "Project file $csproj does not exist." -ForegroundColor Red
        continue
    }

    dotnet pack $csproj -o . -p:PackageVersion=$buildVersion
    Write-Host "Packaged $([System.IO.Path]::GetFileName($csproj)) v$buildVersion" -ForegroundColor Green
}
