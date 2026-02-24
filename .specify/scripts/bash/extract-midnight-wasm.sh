#!/bin/bash

# Extract and validate Midnight WASM binary from npm package
# Usage: ./extract-midnight-wasm.sh [output-directory]

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
OUTPUT_DIR="${1:-$PROJECT_ROOT/src/Sigil.Sdk/Proof}"

NPM_PACKAGE="@midnight-ntwrk/proof-verification"
NPM_VERSION="latest"  # Can be pinned to specific version

echo "=========================================="
echo "Midnight WASM Binary Extraction & Validation"
echo "=========================================="
echo ""
echo "NPM Package: $NPM_PACKAGE@$NPM_VERSION"
echo "Output Dir: $OUTPUT_DIR"
echo ""

# Step 1: Create temporary directory for npm download
TEMP_DIR=$(mktemp -d)
trap "rm -rf $TEMP_DIR" EXIT

echo "[Step 1] Downloading $NPM_PACKAGE..."
cd "$TEMP_DIR"

# Use npm pack to download without installing globally
npm pack "$NPM_PACKAGE@$NPM_VERSION" > /dev/null 2>&1

# Extract the tarball
TARBALL=$(ls -1 *.tgz | head -1)
tar xzf "$TARBALL"

echo "✓ Downloaded: $TARBALL"
echo ""

# Step 2: Locate WASM binary
echo "[Step 2] Locating WASM binary in package..."

# Expected path structure in npm package: package/node_modules or package/dist or package/lib
WASM_FILE=""
for search_path in "package/wasm" "package/dist" "package/lib" "package/dist/wasm" "package/pkg" "package"; do
    if [ -d "$search_path" ]; then
        FOUND=$(find "$search_path" -name "*.wasm" 2>/dev/null | head -1 || true)
        if [ -n "$FOUND" ]; then
            WASM_FILE="$FOUND"
            break
        fi
    fi
done

if [ -z "$WASM_FILE" ]; then
    echo "❌ WASM file not found in package"
    echo "   Searched paths: package/wasm, package/dist, package/lib, package/dist/wasm, package/pkg, package"
    exit 1
fi

echo "✓ Found: $WASM_FILE"
echo ""

# Step 3: Validate file
echo "[Step 3] Validating WASM binary..."

FILE_SIZE=$(stat -f%z "$WASM_FILE" 2>/dev/null || stat -c%s "$WASM_FILE" 2>/dev/null)
echo "  Size: $FILE_SIZE bytes"

# Quick validation: WASM files start with magic bytes: 0x00 0x61 0x73 0x6d
MAGIC=$(xxd -p -l 4 "$WASM_FILE" | head -c 8)
if [ "$MAGIC" != "0061736d" ]; then
    echo "❌ Invalid WASM magic bytes: $MAGIC"
    exit 1
fi
echo "  Magic bytes: ✓ Valid WASM"

# Compute checksum
SHA256=$(shasum -a 256 "$WASM_FILE" | awk '{print $1}')
echo "  SHA256: $SHA256"
echo ""

# Step 4: Copy to output directory
echo "[Step 4] Copying WASM binary to SDK..."
mkdir -p "$OUTPUT_DIR"
BASENAME=$(basename "$WASM_FILE")
cp "$WASM_FILE" "$OUTPUT_DIR/midnight-proof-verification.wasm"
echo "✓ Copied to: $OUTPUT_DIR/midnight-proof-verification.wasm"
echo ""

# Step 5: Create checksum file for verification
CHECKSUM_FILE="$OUTPUT_DIR/midnight-proof-verification.wasm.sha256"
echo "$SHA256  midnight-proof-verification.wasm" > "$CHECKSUM_FILE"
echo "✓ Checksum saved to: $CHECKSUM_FILE"
echo ""

echo "=========================================="
echo "Extraction Complete!"
echo "=========================================="
echo ""
echo "Summary:"
echo "  WASM Binary: $OUTPUT_DIR/midnight-proof-verification.wasm"
echo "  File Size: $FILE_SIZE bytes"
echo "  SHA256: $SHA256"
echo "  Checksum: $CHECKSUM_FILE"
echo ""
echo "Next Steps:"
echo "  1. Review the extracted WASM binary"
echo "  2. Add to .gitignore if binary files are excluded"
echo "  3. Update csproj to embed as EmbeddedResource (T015)"
echo "  4. Run conformance tests to validate latency (T020)"
echo ""
