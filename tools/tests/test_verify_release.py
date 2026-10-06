import json
import shutil
import subprocess
import tempfile
import unittest
from pathlib import Path


REPO = Path(__file__).resolve().parents[2]


@unittest.skipUnless(shutil.which("powershell") and shutil.which("git"), "Windows PowerShell and Git required")
class ReleaseGateTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.write("tools/verify-release.ps1", (REPO / "tools/verify-release.ps1").read_text(encoding="utf-8-sig"))
        self.write("tools/verify-sample-mirrors.ps1", "Write-Host 'Fixture mirrors supplied'\n")
        self.write("LICENSE", "MIT License\n")
        self.write("README.md", "unitytools-ui/v2.0.0\nunitytools-timer/v1.0.0\n")
        for name, version in (("ui", "2.1.0"), ("timer", "1.1.0")):
            package = f"UnityTools/Packages/com.aassder95.unitytools.{name}"
            manifest = {"name": f"com.aassder95.unitytools.{name}", "version": version, "dependencies": {}}
            for field in ("documentationUrl", "changelogUrl", "licensesUrl"):
                manifest[field] = f"https://github.com/aassder95/UnityTools/tree/unitytools-{name}/v{version}/LICENSE"
            self.write(f"{package}/package.json", json.dumps(manifest))
            self.write(f"{package}/CHANGELOG.md", f"## [{version}] - Unreleased\n")
            assembly = "Ui" if name == "ui" else "Timer"
            self.write(f"{package}/Runtime/UnityTools.{assembly}.asmdef", json.dumps({"name": f"UnityTools.{assembly}"}))
        self.write("UnityTools/Packages/com.aassder95.unitytools.ui/Runtime/InputSystem/UnityTools.Ui.InputSystem.asmdef", json.dumps({"references": ["Unity.InputSystem"], "defineConstraints": ["UNITYTOOLS_INPUT_SYSTEM"]}))
        self.git("init", "--quiet")
        self.git("remote", "add", "origin", "git@github.com:aassder95/UnityTools.git")
        self.git("add", ".")
        self.git("-c", "user.name=Fixture", "-c", "user.email=fixture@localhost", "commit", "--quiet", "-m", "test: 배포 검사 fixture")

    def write(self, path, value):
        target = self.root / path
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text(value, encoding="utf-8-sig" if target.suffix == ".ps1" else "utf-8")

    def git(self, *args):
        return subprocess.run(["git", "-C", str(self.root), *args], check=True, capture_output=True)

    def run_gate(self, *args):
        return subprocess.run(["powershell", "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", str(self.root / "tools/verify-release.ps1"), *args], capture_output=True, encoding="utf-8", errors="replace")

    def candidate(self, *args):
        return self.run_gate("-Candidate", "-UiVersion", "2.1.0", "-TimerVersion", "1.1.0", *args)

    def test_candidate_accepts_ssh_and_keeps_existing_install_docs(self):
        result = self.candidate()
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

    def test_candidate_accepts_https(self):
        self.git("remote", "set-url", "origin", "https://github.com/aassder95/UnityTools.git")
        self.assertEqual(self.candidate().returncode, 0)

    def test_candidate_rejects_version_mismatch(self):
        self.assertNotEqual(self.run_gate("-Candidate", "-UiVersion", "2.2.0", "-TimerVersion", "1.1.0").returncode, 0)

    def test_candidate_rejects_wrong_document_version(self):
        path = "UnityTools/Packages/com.aassder95.unitytools.ui/package.json"
        manifest = json.loads((self.root / path).read_text(encoding="utf-8"))
        manifest["documentationUrl"] = manifest["documentationUrl"].replace("v2.1.0", "v2.0.0")
        self.write(path, json.dumps(manifest))
        self.assertNotEqual(self.candidate().returncode, 0)

    def test_candidate_rejects_missing_change_record(self):
        self.write("UnityTools/Packages/com.aassder95.unitytools.timer/CHANGELOG.md", "## Unreleased\n")
        self.assertNotEqual(self.candidate().returncode, 0)

    def test_candidate_rejects_credential_origin(self):
        self.git("remote", "set-url", "origin", "https://fixture:fake-password@github.com/aassder95/UnityTools.git")
        result = self.candidate()
        self.assertNotEqual(result.returncode, 0)
        self.assertNotIn("fake-password", result.stdout + result.stderr)

    def test_modes_cannot_be_combined(self):
        self.assertNotEqual(self.candidate("-Release").returncode, 0)

    def test_release_mode_requires_current_install_docs(self):
        self.assertNotEqual(self.run_gate("-Release", "-UiVersion", "2.1.0", "-TimerVersion", "1.1.0").returncode, 0)
        self.write("README.md", "unitytools-ui/v2.1.0\nunitytools-timer/v1.1.0\n")
        result = self.run_gate("-Release", "-UiVersion", "2.1.0", "-TimerVersion", "1.1.0")
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)

    def test_default_release_contract_is_backward_compatible(self):
        for name, version in (("ui", "2.0.0"), ("timer", "1.0.0")):
            path = f"UnityTools/Packages/com.aassder95.unitytools.{name}/package.json"
            manifest = json.loads((self.root / path).read_text(encoding="utf-8"))
            manifest["version"] = version
            self.write(path, json.dumps(manifest))
        result = self.run_gate("-Release")
        self.assertEqual(result.returncode, 0, result.stdout + result.stderr)


if __name__ == "__main__":
    unittest.main()
