"""Create UPM tarballs from immutable package tags, without copying a working tree."""
import argparse
import gzip
import hashlib
import io
import json
from pathlib import Path
import subprocess
import tarfile


def build(output):
    output.mkdir(parents=True, exist_ok=True)
    records = []
    for name in ("benchmark", "persistence"):
        tag = f"unitytools-{name}/v1.0.0"
        package = f"com.aassder95.unitytools.{name}"
        source = f"UnityTools/Packages/{package}"
        ref = subprocess.check_output(["git", "rev-parse", f"{tag}^{{commit}}"], text=True).strip()
        raw = subprocess.check_output(["git", "archive", "--format=tar", "--prefix=package/", f"{tag}:{source}"])
        normalized = io.BytesIO()
        with tarfile.open(fileobj=io.BytesIO(raw)) as source_archive, tarfile.open(fileobj=normalized, mode="w") as target_archive:
            for member in source_archive.getmembers():
                member.mtime = 0
                member.uid = 0
                member.gid = 0
                member.uname = ""
                member.gname = ""
                member.pax_headers = {}
                target_archive.addfile(member, source_archive.extractfile(member) if member.isfile() else None)
        payload = gzip.compress(normalized.getvalue(), mtime=0)
        with tarfile.open(fileobj=io.BytesIO(payload), mode="r:gz") as archive:
            manifest = json.load(archive.extractfile("package/package.json"))
            if manifest["name"] != package or manifest["version"] != "1.0.0" or manifest["dependencies"] != {}:
                raise ValueError("패키지 배포 계약 불일치")
            license_name = "LICENSE" if name == "benchmark" else "LICENSE.md"
            license_body = archive.extractfile("package/" + license_name).read()
            root_license = subprocess.check_output(["git", "show", f"{tag}:LICENSE"])
            if license_body.replace(b"\r\n", b"\n") != root_license.replace(b"\r\n", b"\n"):
                raise ValueError("MIT 라이선스 불일치")
        filename = f"{package}-1.0.0.tgz"
        (output / filename).write_bytes(payload)
        records.append({"file": filename, "sha256": hashlib.sha256(payload).hexdigest(), "tag": tag, "commit": ref})
    (output / "package-assets.json").write_text(json.dumps(records, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(records, indent=2))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    build(parser.parse_args().output)
