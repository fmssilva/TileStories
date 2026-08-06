$iconsDir = "C:\Users\franc\Desktop\TileStories\TileStories\Assets\Framework\Runtime\UI\Markers\Icons"
$files = @("IconRoyal&Government.png.meta", "IconReligious.png.meta", "IconMilitary.png.meta", "IconNobel&PrivateResidence.png.meta", "IconIndustry&Trade.png.meta", "IconInfrastructures.png.meta")

foreach ($file in $files) {
    $path = Join-Path $iconsDir $file
    if (Test-Path $path) {
        $content = Get-Content $path -Raw
        $content = $content -replace '  nPOTScale: 1', '  nPOTScale: 0'
        $content = $content -replace '  spriteMode: 0', '  spriteMode: 1'
        $content = $content -replace '  alphaIsTransparency: 0', '  alphaIsTransparency: 1'
        $content = $content -replace '  textureType: 0', '  textureType: 8'
        Set-Content -Path $path -Value $content
        Write-Host "Updated: $file"
    } else {
        Write-Host "NOT FOUND: $file"
    }
}
