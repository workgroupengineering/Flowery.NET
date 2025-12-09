#!/usr/bin/env python3
"""
Add missing semantic content colors (Info, Success, Warning, Error) to existing theme palettes.
This script updates the existing AXAML palette files without needing the original CSS sources.
"""

import re
from pathlib import Path


def get_contrasting_color(hex_color: str) -> str:
    """Calculate a contrasting text color (black or white) based on perceived brightness."""
    hex_color = hex_color.lstrip('#')
    r = int(hex_color[0:2], 16)
    g = int(hex_color[2:4], 16)
    b = int(hex_color[4:6], 16)

    # Calculate perceived brightness using the formula:
    # (0.299 * R + 0.587 * G + 0.114 * B)
    brightness = (0.299 * r + 0.587 * g + 0.114 * b) / 255

    # Return black for light backgrounds, white for dark backgrounds
    return '#000000' if brightness > 0.5 else '#FFFFFF'


def extract_color(content: str, key: str) -> str | None:
    """Extract a color value from AXAML content."""
    pattern = rf'<Color x:Key="{key}">([^<]+)</Color>'
    match = re.search(pattern, content)
    return match.group(1) if match else None


def has_resource(content: str, key: str) -> bool:
    """Check if a resource key exists in the content."""
    return f'x:Key="{key}"' in content


def add_content_color(content: str, base_key: str, content_key: str) -> str:
    """Add a content color after the base color's brush."""
    base_color = extract_color(content, f'{base_key}Color')
    if not base_color:
        return content
    
    # Already has content color
    if has_resource(content, f'{content_key}Color'):
        return content
    
    # Calculate contrasting color
    contrast_color = get_contrasting_color(base_color)
    
    # Find the brush line and add content color after it
    brush_pattern = rf'(<SolidColorBrush x:Key="{base_key}Brush"[^/]*/>\r?\n)'
    
    new_lines = (
        f'    <Color x:Key="{content_key}Color">{contrast_color}</Color>\n'
        f'    <SolidColorBrush x:Key="{content_key}Brush" Color="{{StaticResource {content_key}Color}}" />\n'
    )
    
    replacement = rf'\1{new_lines}'
    return re.sub(brush_pattern, replacement, content)


def update_palette_file(file_path: Path) -> bool:
    """Update a single palette file with missing semantic content colors."""
    content = file_path.read_text(encoding='utf-8')
    original = content
    
    # Add missing content colors for semantic variants
    semantic_colors = [
        ('DaisyInfo', 'DaisyInfoContent'),
        ('DaisySuccess', 'DaisySuccessContent'),
        ('DaisyWarning', 'DaisyWarningContent'),
        ('DaisyError', 'DaisyErrorContent'),
    ]
    
    for base_key, content_key in semantic_colors:
        content = add_content_color(content, base_key, content_key)
    
    if content != original:
        file_path.write_text(content, encoding='utf-8')
        return True
    return False


def main():
    script_dir = Path(__file__).parent
    palettes_dir = script_dir.parent / 'Flowery.NET' / 'Themes' / 'Palettes'
    
    if not palettes_dir.exists():
        print(f"Palettes directory not found: {palettes_dir}")
        return
    
    palette_files = sorted(palettes_dir.glob('Daisy*.axaml'))
    print(f"Found {len(palette_files)} palette files")
    
    updated = 0
    for palette_file in palette_files:
        if update_palette_file(palette_file):
            print(f"  Updated: {palette_file.name}")
            updated += 1
        else:
            print(f"  Skipped (already has content colors): {palette_file.name}")
    
    print(f"\nUpdated {updated} palette files")


if __name__ == '__main__':
    main()
