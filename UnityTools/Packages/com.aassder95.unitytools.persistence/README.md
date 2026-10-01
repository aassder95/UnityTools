# UnityTools Persistence

**1.0.0 릴리스 후보를 준비 중이며 공개 tag는 아직 없습니다.** `0.1.0`과 공개 API·저장 envelope format을 유지하며 패키지 업그레이드 자체에 데이터 migration이 필요하지 않습니다.

## 설치

현재 후보는 `Add package from disk...`에서 이 폴더의 `package.json`을 선택합니다. 정식 tag 발행 후 사용할 고정 주소는 다음과 같습니다. 발행 전에는 이 주소로 설치할 수 없습니다.

```text
https://github.com/aassder95/UnityTools.git?path=/UnityTools/Packages/com.aassder95.unitytools.persistence#unitytools-persistence/v1.0.0
```

## Save Recovery Lab

Package Manager의 Samples에서 **Save Recovery Lab**을 Import하면 구버전 변환, 백업 복구, 미래 버전 보호와 세 가지 실패 조건을 재현할 수 있습니다. `SaveRecoveryLab.unity`에서 입력·출력 파일과 실제 API 결과를 비교합니다. 자세한 실행 방법은 [샘플 안내](Samples~/Save%20Recovery%20Lab/README.md)를 참고하세요.

Unity 2022.3 이상에서 쓰는 독립 로컬 저장 패키지입니다. UI와 Timer 패키지에 의존하지 않습니다.

## 책임

- 저장 파일에 양수 버전과 payload, SHA-256 무결성 값을 기록합니다.
- 현재 버전까지 `ISaveMigration`을 한 단계씩 적용하고, 최종 데이터를 게임의 검증 함수로 확인합니다.
- 같은 디렉터리의 임시 파일을 디스크에 flush한 뒤 처음 저장할 때는 이동하고, 기존 파일은 `File.Replace`로 교체합니다. 정상 교체 시 이전 파일을 `.bak`으로 보관합니다.
- 기본 파일이 손상되면 검증 가능한 백업을 읽습니다. 이 상태에서 다시 저장할 때는 유효한 백업을 손상된 기본 파일로 덮어쓰지 않습니다.
- 더 높은 버전의 기본 파일을 발견하면 이전 버전 앱이 이를 덮어쓰지 않도록 저장을 거부합니다.

게임의 저장 모델·데이터 검증·V1→V2 변환 내용은 프로젝트가 소유합니다. 해시는 우발적 손상을 탐지하기 위한 것이며 변조 방지나 암호화 수단이 아닙니다.

## 최소 사용 예제

다음 모델을 `ProgressData.cs`에 둡니다.

```csharp
using System;
using UnityEngine;

[Serializable]
public class ProgressData
{
    [SerializeField] private int _level;
    public int Level => _level;

    public ProgressData(int level)
    {
        _level = level;
    }
}
```

초기화나 명시적인 로드 처리에서 다음과 같이 사용합니다. `System.IO`, `UnityEngine`, `UnityTools.Persistence` namespace가 필요합니다.

```csharp
string path = Path.Combine(Application.persistentDataPath, "progress.json");
if (!VersionedSaveStore<ProgressData>.TryCreate(path, 1, new UnityJsonSaveCodec<ProgressData>(), value => value.Level >= 0, null, out VersionedSaveStore<ProgressData> saves))
    return;

if (!saves.TryLoad(out ProgressData data, out bool wasRecovered, out bool wasMigrated))
{
    // 파일 없음·손상·미래 버전 등을 구분할 사용자 흐름으로 이동합니다.
    // false만으로 새 게임을 저장하면 기존 데이터가 덮어써질 수 있습니다.
    return;
}

// data로 게임 상태를 구성한 뒤 변환/복구 내용을 저장할 시점을 결정합니다.
if ((wasRecovered || wasMigrated) && !saves.TrySave(data))
{
    // 저장 실패를 호출자에게 알리고 재시도 또는 사용자 선택을 제공합니다.
}
```

`TryLoad`의 `false`는 새 설치로 파일이 없는 경우와 복구 불가능한 실패를 모두 포함합니다. 게임에서는 새 게임 생성 전에 파일 존재 여부나 별도 사용자 흐름을 확인하세요. 저장 실패 시 기존 파일을 성공으로 처리하지 말고 호출자에게 알려야 합니다.

`UnityJsonSaveCodec<T>`는 Unity `JsonUtility`의 직렬화 규칙을 따릅니다. Dictionary·다형성·property 중심 데이터 등은 프로젝트의 `ISaveCodec<T>` 구현을 사용하세요. Migration은 이전 payload를 받아 다음 버전 payload를 반환하며, 누락된 단계나 최종 검증 실패는 로드를 중단합니다. 저장 모델의 새 enum 값은 문자열로 저장하는 편이 변경에 안전합니다.

## 확인할 환경

[API 계약](Documentation~/api.md)에 실패 출력·미래 버전 보호의 해제 조건·codec/migration 요구 사항과 파일 API를 정리했습니다.

- 같은 경로에는 단일 writer만 사용합니다. store는 thread-safe하지 않으며 동시 저장·다중 프로세스를 지원하지 않습니다.
- JSON·hash·파일 입출력은 동기 실행입니다. 데이터 크기와 저장 시점은 호출자가 관리하며 프레임 반복 호출에 적합하지 않습니다.
- codec·migration·검증 함수는 예외를 발생시키지 않고 파일·게임 상태를 변경하지 않아야 합니다. store는 이들의 예외를 대신 삼키지 않습니다.
- 미래 버전 **본문**은 로드 전에 저장해도 보호됩니다. 백업 전체 이력의 버전 보호나 외부 보상 transaction은 제공하지 않습니다.

파일 교체는 파일 시스템과 플랫폼의 `File.Replace` 지원에 의존하며 지원되지 않으면 `TrySave`가 실패합니다. 배포 대상 Android 기기와 IL2CPP 빌드에서 저장·백업·강제 종료 후 복구를 별도로 확인하세요. 새 프로젝트에는 `Package Manager > Add package from disk...`에서 이 폴더의 `package.json`을 선택합니다.

검증 대상은 Unity `2022.3.62f3`과 `6000.3.20f1`의 Windows Editor Play Mode 및 Windows Development Build(Mono)입니다. Android/iOS·IL2CPP·WebGL·실기기 강제 종료 복구는 아직 검증하지 않았습니다. 모든 저장 실패나 전원 손실에 대한 원자적 transaction 보장은 아닙니다. 상세 후보 검증과 tag 재검증 절차는 저장소의 `docs/RELEASE_PERSISTENCE.md`에 있습니다.

## 라이선스

[MIT License](LICENSE.md). UPM 배포 폴더에 라이선스 원문을 포함합니다.
