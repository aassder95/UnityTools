"""Check first-party UPM distribution contracts without launching Unity."""

import argparse
import json
import re
from pathlib import Path


PACKAGES = {
    "ui": "UnityTools.Ui",
    "timer": "UnityTools.Timer",
    "benchmark": "UnityTools.Benchmark",
    "persistence": "UnityTools.Persistence",
    "vfx": "UnityTools.Vfx.Editor",
}
FORBIDDEN = re.compile(r"DOTween|Com\.ForbiddenByte|(?:^|/)OSA(?:/|$)", re.I)


def validate(root):
    errors = []
    packages = root / "UnityTools" / "Packages"
    assemblies = {}
    references_to_check = []

    def reject(path, message):
        errors.append(f"{path.relative_to(root).as_posix()}: {message}")

    def read_text(path):
        try:
            return path.read_text(encoding="utf-8-sig")
        except (OSError, UnicodeError) as error:
            reject(path, f"Unreadable UTF-8 file ({type(error).__name__})")
            return None

    def read_json(path):
        content = read_text(path)
        if content is None:
            return {}
        try:
            data = json.loads(content)
            if not isinstance(data, dict):
                reject(path, "JSON must be an object")
                return {}
            return data
        except ValueError as error:
            reject(path, f"Invalid JSON ({type(error).__name__})")
            return {}

    for suffix, assembly in PACKAGES.items():
        package = packages / f"com.aassder95.unitytools.{suffix}"
        manifest_path = package / "package.json"
        manifest = read_json(manifest_path)
        if manifest.get("name") != package.name:
            reject(manifest_path, "Package name must match directory")
        if not re.fullmatch(r"\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?", str(manifest.get("version", ""))):
            reject(manifest_path, "Invalid semantic version")
        if manifest.get("unity") != "2022.3":
            reject(manifest_path, "Minimum Unity version contract is 2022.3")
        dependencies = manifest.get("dependencies")
        if not isinstance(dependencies, dict):
            reject(manifest_path, "Dependencies must be an object")
        elif suffix == "ui":
            if set(dependencies) != {"com.unity.ugui", "com.unity.textmeshpro"}:
                reject(manifest_path, "UI core dependencies must be uGUI and TMP only")
        elif dependencies:
            reject(manifest_path, "Core package must have no external dependencies")

        sample_roots = set()
        samples = manifest.get("samples", [])
        if not isinstance(samples, list):
            reject(manifest_path, "Samples must be an array")
            samples = []
        sample_names = set()
        for sample in samples:
            if not isinstance(sample, dict):
                reject(manifest_path, "Sample must be an object")
                continue
            name = sample.get("displayName")
            if not isinstance(name, str) or not name.strip() or name in sample_names:
                reject(manifest_path, "Missing or duplicate sample displayName")
            else:
                sample_names.add(name)
            relative = sample.get("path", "")
            if not isinstance(relative, str):
                reject(manifest_path, "Sample path must be a string")
                continue
            if "\0" in relative:
                reject(manifest_path, "Invalid sample path")
                continue
            try:
                sample_path = (package / relative).resolve()
            except (OSError, ValueError):
                reject(manifest_path, "Invalid sample path")
                continue
            if not sample_path.is_relative_to((package / "Samples~").resolve()) or sample_path == (package / "Samples~").resolve():
                reject(manifest_path, "Sample path must stay inside Samples~")
                continue
            sample_roots.add(sample_path)
            if not sample_path.is_dir() or not (sample_path / "README.md").is_file() or not list(sample_path.rglob("*.unity")):
                reject(manifest_path, f"Sample requires directory, README and scene: {relative}")

        for document in ("README.md", "package.json"):
            if not (package / document).is_file():
                reject(package / document, "Required distribution file is missing")

        guids = {}
        for path in sorted(package.rglob("*")):
            relative = path.relative_to(package).as_posix()
            if FORBIDDEN.search(relative):
                reject(path, "Vendor files must not be distributed in first-party packages")
            if path.name == "Samples~" or path.resolve() in sample_roots:
                continue
            if path.suffix == ".meta":
                owner = path.with_suffix("")
                if not owner.exists():
                    reject(path, "Orphan meta file")
                content = read_text(path)
                if content is None:
                    continue
                matches = re.findall(r"^guid: ([0-9a-f]{32})$", content, re.M)
                if len(matches) != 1:
                    reject(path, "Meta must contain exactly one valid GUID")
                elif matches[0] in guids:
                    reject(path, f"Duplicate GUID owned by {guids[matches[0]]}")
                else:
                    guids[matches[0]] = relative
                continue
            if path.is_file() and path.suffix not in {".md", ".txt"} and not Path(str(path) + ".meta").is_file():
                reject(path, "Missing meta file")
            if path.suffix == ".asmdef":
                definition = read_json(path)
                references = definition.get("references", [])
                if not isinstance(references, list) or any(not isinstance(value, str) for value in references):
                    reject(path, "Assembly references must be an array of strings")
                    references = []
                constraints = definition.get("defineConstraints", [])
                if not isinstance(constraints, list) or any(not isinstance(value, str) for value in constraints):
                    reject(path, "Assembly constraints must be an array of strings")
                    constraints = []
                references_to_check.append((path, references))
                name = definition.get("name")
                if suffix == "vfx" and definition.get("includePlatforms") != ["Editor"]:
                    reject(path, "VFX package assemblies must be Editor-only")
                if suffix == "vfx" and name == assembly and references:
                    reject(path, "VFX editor assembly must have no external references")
                if not isinstance(name, str) or not name:
                    reject(path, "Assembly requires a name")
                elif name in assemblies:
                    reject(path, "Duplicate assembly name")
                else:
                    assemblies[name] = path
                if "Runtime" in path.relative_to(package).parts and "Samples~" not in path.relative_to(package).parts:
                    if name == assembly and references:
                        reject(path, "Core runtime assembly must have no external references")
                    elif suffix == "ui" and name == "UnityTools.Ui.InputSystem":
                        if "Unity.InputSystem" not in references or "UNITYTOOLS_INPUT_SYSTEM" not in constraints:
                            reject(path, "Input System must remain a constrained optional assembly")
            if path.suffix == ".cs":
                if suffix == "vfx" and "Editor" not in path.relative_to(package).parts:
                    reject(path, "VFX package code must stay in Editor folders")
                content = read_text(path)
                if content is not None and re.search(r"UnityTools\.Util|OSA\.Core|Com\.ForbiddenByte|DOTweenPro", content):
                    reject(path, "Legacy or vendor namespace in package code")

        expected_assembly = package / ("Editor" if suffix == "vfx" else "Runtime") / f"{assembly}.asmdef"
        if read_json(expected_assembly).get("name") != assembly:
            reject(expected_assembly, "Core assembly name does not match contract")

    for path, references in references_to_check:
        for reference in references:
            if reference.startswith("UnityTools.") and reference not in assemblies:
                reject(path, f"Unknown first-party assembly reference: {reference}")

    return errors


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument("--report", type=Path, help="Write static validation results as UTF-8 JSON")
    args = parser.parse_args()
    errors = validate(args.root.resolve())
    if args.report is not None:
        report = {
            "schema_version": 1,
            "status": "failed" if errors else "passed",
            "packages": [f"com.aassder95.unitytools.{suffix}" for suffix in PACKAGES],
            "error_count": len(errors),
            "errors": errors,
            "unity_runtime_tested": False,
        }
        try:
            args.report.parent.mkdir(parents=True, exist_ok=True)
            args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        except OSError as error:
            print(f"Cannot write static report ({type(error).__name__}): {args.report}")
            return 1
    for error in errors:
        print(error)
    if errors:
        print(f"UPM static checks failed: {len(errors)} issue(s).")
        return 1
    print(f"UPM static checks passed: {len(PACKAGES)} packages (Unity runtime not tested).")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
