#!/bin/bash
# Setup script to download E5-Small-V2 model and tokenizer files

set -e

MODEL_DIR="VectorDataBase/MLModels/e5-small-v2"
BASE_URL="https://huggingface.co/intfloat/e5-small-v2/resolve/main"

# Files to download
declare -A files=(
    ["model.onnx"]="133 MB"
    ["vocab.txt"]="232 KB"
    ["tokenizer.json"]="712 KB"
    ["tokenizer_config.json"]="363 bytes"
)

echo "Setting up E5-Small-V2 model and tokenizer..."
echo ""

# Create directory if it doesn't exist
if [ ! -d "$MODEL_DIR" ]; then
    echo "Creating directory: $MODEL_DIR"
    mkdir -p "$MODEL_DIR"
fi

# Download each file
for file in "${!files[@]}"; do
    output_path="$MODEL_DIR/$file"
    
    if [ -f "$output_path" ]; then
        echo "✓ $file already exists, skipping..."
        continue
    fi
    
    url="$BASE_URL/$file"
    echo "Downloading $file (${files[$file]})..."
    
    if curl -L -o "$output_path" "$url"; then
        echo "✓ Downloaded $file successfully"
    else
        echo "✗ Failed to download $file"
        exit 1
    fi
done

echo ""
echo "✓ All files downloaded successfully!"
echo "You can now build and run the project."
