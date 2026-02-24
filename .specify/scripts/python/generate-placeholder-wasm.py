#!/usr/bin/env python3
"""
Generate a minimal valid WASM binary with required exports for Midnight PoC.
This placeholder is used until real Midnight WASM is extracted via extract-midnight-wasm.sh

Magic bytes (0x00617361 = "\0asm") + version + minimal type/function sections.
"""

import struct

def create_minimal_wasm_binary():
    """Create minimal WASM binary with exports for Midnight PoC testing."""
    
    wasm = bytearray()
    
    # 1. Magic number and version
    wasm.extend(b'\x00\x61\x73\x6d')  # Magic: \0asm
    wasm.extend(struct.pack('<I', 1))  # Version: 1
    
    # 2. Type section (empty function types)
    # Section 1: Type section ID=1
    wasm.append(1)  # Section ID
    wasm.append(1)  # Length: 1 byte (vec(functiontype) with 0 types)
    wasm.append(0)  # 0 function types
    
    # 3. Import section (import memory from env)
    # Section 2: Import section ID=2
    import_section = bytearray()
    import_section.append(1)  # 1 import
    
    # Import name: "env"
    import_section.append(3)  # String length
    import_section.extend(b'env')
    
    # Import name: "memory"
    import_section.append(6)  # String length
    import_section.extend(b'memory')
    
    # Import kind: Memory (3)
    import_section.append(3)
    # Memory descriptor: initial=256 pages (16MB), no max
    import_section.append(0)  # No limit
    import_section.append(128)  # 128 + 128 = 256 pages (little-endian varint)
    import_section.append(2)
    
    wasm.append(2)  # Section ID
    wasm.append(len(import_section))  # Section length
    wasm.extend(import_section)
    
    # 4. Function section (define verify_proof function)
    # Section 3: Function section ID=3
    function_section = bytearray()
    function_section.append(1)  # 1 function
    function_section.append(0)  # Function type 0 (i32, i32, i32, i32 -> i32)
    
    wasm.append(3)  # Section ID
    wasm.append(len(function_section))  # Section length
    wasm.extend(function_section)
    
    # 5. Export section (export verify_proof, get_last_error, get_version, memory)
    export_section = bytearray()
    export_section.append(4)  # 4 exports: verify_proof, get_last_error, get_version, memory
    
    # Export 1: verify_proof (function)
    export_section.append(12)  # String length
    export_section.extend(b'verify_proof')
    export_section.append(0)  # Type: function
    export_section.append(0)  # Function index 0
    
    # Export 2: get_last_error (function)
    export_section.append(15)  # String length
    export_section.extend(b'get_last_error')
    export_section.append(0)  # Type: function
    export_section.append(1)  # Function index 1 (placeholder)
    
    # Export 3: get_version (function)
    export_section.append(11)  # String length
    export_section.extend(b'get_version')
    export_section.append(0)  # Type: function
    export_section.append(2)  # Function index 2 (placeholder)
    
    # Export 4: memory (memory)
    export_section.append(6)  # String length
    export_section.extend(b'memory')
    export_section.append(3)  # Type: memory
    export_section.append(0)  # Memory index 0
    
    wasm.append(7)  # Section ID
    wasm.append(len(export_section))  # Section length
    wasm.extend(export_section)
    
    # 6. Code section (minimal function bodies that return error code)
    code_section = bytearray()
    code_section.append(1)  # 1 function body
    
    # Function body for verify_proof: just return error code 999 (InternalError)
    # (This is a placeholder; real Midnight will verify actual proofs)
    func_body = bytearray()
    func_body.append(0x41)  # i32.const
    func_body.append(0xE7)  # 999 as varint (0xE7 0x07 = 999)
    func_body.append(0x07)
    func_body.append(0x0B)  # end
    
    code_section.append(len(func_body))  # Function body length
    code_section.extend(func_body)
    
    wasm.append(10)  # Section ID
    wasm.append(len(code_section))  # Section length
    wasm.extend(code_section)
    
    return bytes(wasm)


if __name__ == '__main__':
    import sys
    import os
    
    wasm_binary = create_minimal_wasm_binary()
    
    # Determine output path
    if len(sys.argv) > 1:
        output_path = sys.argv[1]
    else:
        # Default: src/Sigil.Sdk/Proof/midnight-proof-verification.wasm
        script_dir = os.path.dirname(os.path.abspath(__file__))
        output_path = os.path.join(
            script_dir, '..', 'src', 'Sigil.Sdk', 'Proof', 'midnight-proof-verification.wasm'
        )
    
    # Ensure directory exists
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    
    # Write binary
    with open(output_path, 'wb') as f:
        f.write(wasm_binary)
    
    print(f"✓ Created placeholder WASM binary: {output_path}")
    print(f"  Size: {len(wasm_binary)} bytes")
    print(f"  Magic bytes: {wasm_binary[:4].hex()}")
