import importlib.util
import json
import shutil
import subprocess
import sys
import tempfile
import unittest
from pathlib import Path


REPO = Path(__file__).resolve().parents[2]
SPEC = importlib.util.spec_from_file_location("verify_upm", REPO / "tools" / "verify-upm.py")
VALIDATOR = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(VALIDATOR)


class DistributionTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        shutil.copytree(REPO / "UnityTools" / "Packages", self.root / "UnityTools" / "Packages")
        self.package = self.root / "UnityTools" / "Packages" / "com.aassder95.unitytools.timer"

    def change_json(self, path, change):
        value = json.loads(path.read_text(encoding="utf-8-sig"))
        change(value)
        path.write_text(json.dumps(value), encoding="utf-8")

    def assert_rejected(self, text):
        self.assertTrue(any(text in error for error in VALIDATOR.validate(self.root)), text)

    def test_current_distribution_passes(self):
        self.assertEqual(VALIDATOR.validate(self.root), [])

    def test_invalid_json_is_reported(self):
        (self.package / "package.json").write_text("{", encoding="utf-8")
        self.assert_rejected("Invalid JSON")

    def test_sample_cannot_escape_package(self):
        self.change_json(self.package / "package.json", lambda data: data["samples"][0].update(path="../../outside"))
        self.assert_rejected("Sample path must stay inside Samples~")

    def test_runtime_dependency_breaks_independence(self):
        self.change_json(self.package / "package.json", lambda data: data["dependencies"].update({"com.unity.inputsystem": "1.0.0"}))
        self.assert_rejected("Core package must have no external dependencies")

    def test_missing_meta_is_detected(self):
        (self.package / "Runtime" / "TaskTimerService.cs.meta").unlink()
        self.assert_rejected("Missing meta file")

    def test_duplicate_guid_is_detected(self):
        source = self.package / "Runtime" / "TaskTimerService.cs.meta"
        target = self.package / "Runtime" / "PeriodTimerService.cs.meta"
        target.write_bytes(source.read_bytes())
        self.assert_rejected("Duplicate GUID")

    def test_forbidden_vendor_is_detected(self):
        (self.package / "Runtime" / "DOTweenPro.dll").write_bytes(b"fixture")
        self.assert_rejected("Vendor files")

    def test_external_runtime_reference_is_detected(self):
        self.change_json(self.package / "Runtime" / "UnityTools.Timer.asmdef", lambda data: data.update(references=["UnityTools.Ui"]))
        self.assert_rejected("Core runtime assembly must have no external references")

    def test_optional_input_constraint_is_required(self):
        path = self.root / "UnityTools/Packages/com.aassder95.unitytools.ui/Runtime/InputSystem/UnityTools.Ui.InputSystem.asmdef"
        self.change_json(path, lambda data: data.update(defineConstraints=[]))
        self.assert_rejected("constrained optional assembly")

    def test_unknown_first_party_assembly_is_detected(self):
        path = self.package / "Tests/Runtime/UnityTools.Timer.Tests.asmdef"
        self.change_json(path, lambda data: data.update(references=["UnityTools.DoesNotExist"]))
        self.assert_rejected("Unknown first-party assembly")

    def test_null_input_constraints_are_reported(self):
        path = self.root / "UnityTools/Packages/com.aassder95.unitytools.ui/Runtime/InputSystem/UnityTools.Ui.InputSystem.asmdef"
        self.change_json(path, lambda data: data.update(defineConstraints=None))
        self.assert_rejected("Assembly constraints must be an array of strings")
        self.assert_rejected("constrained optional assembly")

    def test_non_string_references_are_reported(self):
        path = self.package / "Tests/Runtime/UnityTools.Timer.Tests.asmdef"
        self.change_json(path, lambda data: data.update(references=[None]))
        self.assert_rejected("Assembly references must be an array of strings")

    def test_invalid_sample_path_is_reported(self):
        self.change_json(self.package / "package.json", lambda data: data["samples"][0].update(path="Samples~/\0"))
        self.assert_rejected("Invalid sample path")

    def test_invalid_utf8_is_reported(self):
        for relative in ("package.json", "Runtime/TaskTimerService.cs", "Runtime/PeriodTimerService.cs.meta"):
            (self.package / relative).write_bytes(b"\xff")
        errors = VALIDATOR.validate(self.root)
        for relative in ("package.json", "Runtime/TaskTimerService.cs", "Runtime/PeriodTimerService.cs.meta"):
            self.assertTrue(any(relative in error and "Unreadable UTF-8" in error for error in errors), relative)

    def test_cli_writes_success_report(self):
        report_path = self.root / "reports" / "result.json"
        result = subprocess.run([sys.executable, str(REPO / "tools/verify-upm.py"), "--root", str(self.root), "--report", str(report_path)], capture_output=True, text=True)
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
        report = json.loads(report_path.read_text(encoding="utf-8"))
        self.assertEqual(report["schema_version"], 1)
        self.assertEqual(report["status"], "passed")
        self.assertEqual(len(report["packages"]), len(VALIDATOR.PACKAGES))
        self.assertEqual(report["error_count"], 0)
        self.assertEqual(report["errors"], [])
        self.assertFalse(report["unity_runtime_tested"])

    def test_cli_writes_failure_report_and_fails(self):
        (self.package / "Runtime" / "TaskTimerService.cs.meta").unlink()
        report_path = self.root / "result.json"
        result = subprocess.run([sys.executable, str(REPO / "tools/verify-upm.py"), "--root", str(self.root), "--report", str(report_path)], capture_output=True, text=True)
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        report = json.loads(report_path.read_text(encoding="utf-8"))
        self.assertEqual(report["status"], "failed")
        self.assertEqual(report["error_count"], len(report["errors"]))
        self.assertTrue(any("TaskTimerService.cs: Missing meta file" in error for error in report["errors"]))
        self.assertFalse(report["unity_runtime_tested"])

    def test_cli_fails_when_report_cannot_be_written(self):
        result = subprocess.run([sys.executable, str(REPO / "tools/verify-upm.py"), "--root", str(self.root), "--report", str(self.root)], capture_output=True, text=True)
        self.assertEqual(result.returncode, 1, result.stdout + result.stderr)
        self.assertIn("Cannot write static report", result.stdout)

    def test_vfx_editor_assembly_cannot_become_player_assembly(self):
        path = self.root / "UnityTools/Packages/com.aassder95.unitytools.vfx/Editor/UnityTools.Vfx.Editor.asmdef"
        self.change_json(path, lambda data: data.update(includePlatforms=[]))
        self.assert_rejected("VFX package assemblies must be Editor-only")

    def test_vfx_code_cannot_escape_editor_folder(self):
        package = self.root / "UnityTools/Packages/com.aassder95.unitytools.vfx"
        source = package / "Editor/VfxPrefabInfo.cs"
        source.rename(package / "VfxPrefabInfo.cs")
        source.with_suffix(".cs.meta").rename(package / "VfxPrefabInfo.cs.meta")
        self.assert_rejected("VFX package code must stay in Editor folders")


if __name__ == "__main__":
    unittest.main()
