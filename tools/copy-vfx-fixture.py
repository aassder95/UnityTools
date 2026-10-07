"""Copy a local visual-only Unity fixture and GUID dependencies without changing its source."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--assets', type=Path, required=True)
    parser.add_argument('--project', type=Path, required=True)
    parser.add_argument('paths', nargs='+')
    args = parser.parse_args()
    root = args.assets.resolve()
    project = args.project.resolve()
    if project == root or root in project.parents or project in root.parents:
        parser.error('Source and destination must be separate directories.')
    guids = {}
    for meta in root.rglob('*.meta'):
        match = re.search(r'^guid: ([a-f0-9]{32})', meta.read_text(encoding='utf-8-sig'), re.M)
        if match:
            guids[match[1]] = Path(str(meta)[:-5])
    pending = [(root / path).resolve() for path in args.paths]
    copied = {}
    while pending:
        source = pending.pop()
        relative = source.relative_to(root)
        if str(relative) in copied:
            continue
        if source.suffix.lower() not in {'.prefab', '.mat', '.shader', '.cginc', '.hlsl', '.png', '.tga', '.jpg', '.jpeg', '.psd', '.fbx', '.asset'}:
            parser.error(f'Unsupported visual dependency: {relative}')
        destination = project / 'Assets' / 'ProjectVfx' / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        for path, target in [(source, destination), (Path(str(source) + '.meta'), Path(str(destination) + '.meta'))]:
            shutil.copy2(path, target)
            copied[str(path.relative_to(root))] = hashlib.sha256(path.read_bytes()).hexdigest()
        if source.suffix.lower() in {'.prefab', '.mat', '.asset', '.shader', '.hlsl', '.cginc'}:
            content = source.read_text(encoding='utf-8-sig')
            for guid in re.findall(r'guid: ([a-f0-9]{32})', content):
                if guid.startswith('0000000000000000'):
                    continue
                if guid not in guids:
                    parser.error(f'Missing GUID {guid} in {relative}')
                pending.append(guids[guid])
            for include in re.findall(r'#include\s+"([^"]+)"', content):
                local = (source.parent / include).resolve()
                if local.is_file():
                    pending.append(local)
                elif '/' in include or '\\' in include:
                    parser.error(f'Unsupported shader include {include} in {relative}')
    report = {'source_assets': str(root), 'sha256': copied, 'entries': args.paths}
    (project / 'project-vfx-source.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
    print(f'Copied {len(copied)} source files including metadata.')


if __name__ == '__main__':
    main()
