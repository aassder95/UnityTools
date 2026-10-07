import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest


SCRIPT = Path(__file__).resolve().parents[1] / 'copy-vfx-fixture.py'


class CopyVfxFixtureTests(unittest.TestCase):
    def test_preserves_guid_dependencies_and_source_bytes(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder) / 'source'
            root.mkdir()
            (root / 'Fx.prefab').write_text('guid: ' + 'a' * 32, encoding='utf-8')
            (root / 'Fx.prefab.meta').write_text('guid: ' + 'b' * 32, encoding='utf-8')
            (root / 'Fx.mat').write_text('guid: 0000000000000000f000000000000000', encoding='utf-8')
            (root / 'Fx.mat.meta').write_text('guid: ' + 'a' * 32, encoding='utf-8')
            source = {path.name: path.read_bytes() for path in root.iterdir()}
            target = Path(folder) / 'project'
            target.mkdir()
            result = subprocess.run([sys.executable, str(SCRIPT), '--assets', str(root), '--project', str(target), 'Fx.prefab'], capture_output=True)
            self.assertEqual(result.returncode, 0, result.stderr)
            for name, content in source.items():
                self.assertEqual((root / name).read_bytes(), content)
                self.assertEqual((target / 'Assets' / 'ProjectVfx' / name).read_bytes(), content)
            report = json.loads((target / 'project-vfx-source.json').read_text(encoding='utf-8'))
            self.assertEqual(len(report['sha256']), 4)

    def test_rejects_source_destination_overlap(self):
        with tempfile.TemporaryDirectory() as folder:
            result = subprocess.run([sys.executable, str(SCRIPT), '--assets', folder, '--project', str(Path(folder) / 'project'), 'Fx.prefab'], capture_output=True)
            self.assertNotEqual(result.returncode, 0)
            self.assertFalse((Path(folder) / 'project').exists())

    def test_rejects_missing_dependency(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder) / 'source'
            root.mkdir()
            (root / 'Fx.prefab').write_text('guid: ' + 'a' * 32, encoding='utf-8')
            (root / 'Fx.prefab.meta').write_text('guid: ' + 'b' * 32, encoding='utf-8')
            target = Path(folder) / 'project'
            target.mkdir()
            result = subprocess.run([sys.executable, str(SCRIPT), '--assets', str(root), '--project', str(target), 'Fx.prefab'], capture_output=True)
            self.assertNotEqual(result.returncode, 0)
            self.assertIn(b'Missing GUID', result.stderr)
            self.assertFalse((target / 'project-vfx-source.json').exists())


if __name__ == '__main__':
    unittest.main()
