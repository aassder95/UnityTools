# Security and History Cleanup

정리 일자: 2026-07-24

## Credential

- Git remote URL에서 평문 GitHub PAT 노출을 확인한 즉시 토큰을 폐기했습니다.
- 토큰 값은 문서, commit, 로그에 기록하지 않았습니다.
- `origin`은 credential이 없는 `https://github.com/aassder95/UnityTools.git`로 교체했습니다.
- 이후 인증은 Git Credential Manager를 사용합니다.

## History rewrite

`git-filter-repo 2.47.0`으로 모든 로컬 branch와 tag 이력에서 다음 경로를 제거했습니다.

- `Com.ForbiddenByte/OSA` 전체와 OSA sample/scene/asmdef
- 기존 출처 불명 DOTween tree
- `DOTweenPro`

오염 파일을 보관하는 별도 backup repository는 만들지 않았습니다.

| Ref | 정리 전 원격 SHA | 정리 후 확인 SHA |
| --- | --- | --- |
| `develop` | `68eb233cf26887c711c703fec3311411966f8607` | `68c32c3b689dc91305ca24ed1f99bf835919b0f2` (릴리스 문서 반영 전 검증 기준) |
| `main` | `9f76f736d897261ef94a971c515d6446277a29ec` | `9f76f736d897261ef94a971c515d6446277a29ec` |
| `unitytools-core/v0.1.0` | `34c55c2805406e0e1d897f5bd1aa317e8dee2198` | `5d392bf8f4800c22c5c8e90ea6a727446d4351f9` |

정리 후 `git rev-list --objects --all` 기준 OSA, `Com.ForbiddenByte`, `DOTweenPro` 경로는 0건입니다. 이력 재작성으로 기존 commit SHA가 바뀌었으므로 정리 이전 clone은 폐기하고 새로 clone해야 합니다. 이전 clone의 branch를 새 이력에 merge하거나 push하면 제거된 이력이 다시 유입될 수 있습니다.

원격 branch/tag 갱신은 기록한 정리 전 SHA를 lease 기준으로 사용하며, 원격 ref가 움직였으면 공개와 force push를 중단합니다.
