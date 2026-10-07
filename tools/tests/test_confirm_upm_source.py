import json
from pathlib import Path
import subprocess
import tempfile
import unittest


SCRIPT = Path(__file__).resolve().parents[1] / 'confirm-upm-source.ps1'
PACKAGE = 'com.aassder95.unitytools.sheets'
REF = 'a' * 40


class ConfirmUpmSourceTests(unittest.TestCase):
    def run_fixture(self, mode):
        with tempfile.TemporaryDirectory() as folder:
            project = Path(folder)
            (project / 'Packages').mkdir()
            url = f'https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/{PACKAGE}#{REF}'
            (project / 'Packages' / 'manifest.json').write_text(json.dumps({'dependencies': {PACKAGE: url}}), encoding='utf-8')
            entry = {'source': 'git', 'version': url, 'hash': REF if mode != 'wrong_hash' else 'b' * 40}
            (project / 'Packages' / 'packages-lock.json').write_text(json.dumps({'dependencies': {PACKAGE: entry}}), encoding='utf-8')
            cache = project / 'Library' / 'PackageCache' / (PACKAGE + '@aaaaaaaaaa')
            cache.mkdir(parents=True)
            package = {'name': PACKAGE, 'version': '0.1.0' if mode != 'wrong_version' else '9.9.9', 'dependencies': {}}
            (cache / 'package.json').write_text(json.dumps(package), encoding='utf-8')
            result = subprocess.run(['powershell', '-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', str(SCRIPT), '-Project', folder, '-PackageName', PACKAGE, '-SourceRef', REF], capture_output=True)
            self.assertEqual(result.returncode == 0, mode == 'valid', result.stderr)
            self.assertEqual((project / 'git-source-result.json').exists(), mode == 'valid')

    def test_valid_git_install(self):
        self.run_fixture('valid')

    def test_rejects_wrong_hash(self):
        self.run_fixture('wrong_hash')

    def test_rejects_wrong_version(self):
        self.run_fixture('wrong_version')
