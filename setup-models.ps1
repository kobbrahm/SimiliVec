#!/usr/bin/env pwsh
# Setup script to download E5-Small-V2 model and tokenizer files

$ErrorActionPreference = "Stop"

$modelDir = "VectorDataBase/MLModels/e5-small-v2"
$baseUrl = "https://huggingface.co/intfloat/e5-small-v2/resolve/main"

# Files to download
$files = @(
    @{ Name = "model.onnx"; Size = "133 MB" },
    @{ Name = "vocab.txt"; Size = "232 KB" },
    @{ Name = "tokenizer.json"; Size = "712 KB" },
    @{ Name = "tokenizer_config.json"; Size = "363 bytes" }
)

Write-Host "Setting up E5-Small-V2 model and tokenizer..." -ForegroundColor Cyan
Write-Host ""

# Create directory if it doesn't exist
if (-not (Test-Path $modelDir)) {
    Write-Host "Creating directory: $modelDir" -ForegroundColor Yellow
    New-Item -ItemType Directory -Force -Path $modelDir | Out-Null
}

# Download each file
foreach ($file in $files) {
    $fileName = $file.Name
    $outputPath = Join-Path $modelDir $fileName
    
    if (Test-Path $outputPath) {
        Write-Host "[OK] $fileName already exists, skipping..." -ForegroundColor Green
        continue
    }
    
    $url = "$baseUrl/$fileName"
    Write-Host "Downloading $fileName ($($file.Size))..." -ForegroundColor Yellow
    
    try {
        Invoke-WebRequest -Uri $url -OutFile $outputPath -UseBasicParsing
        Write-Host "[OK] Downloaded $fileName successfully" -ForegroundColor Green
    }
    catch {
        Write-Host "[ERROR] Failed to download $fileName" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "[SUCCESS] All files downloaded successfully!" -ForegroundColor Green
Write-Host "You can now build and run the project." -ForegroundColor Cyan
