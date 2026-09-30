# UnityTools Persistence

## Save Recovery Lab

Package Manager의 Samples에서 **Save Recovery Lab**을 Import하면 구버전 변환, 백업 복구, 미래 버전 보호와 세 가지 실패 조건을 재현할 수 있습니다. `SaveRecoveryLab.unity`에서 입력·출력 파일과 실제 API 결과를 비교합니다. 자세한 실행 방법은 [샘플 안내](Samples~/Save%20Recovery%20Lab/README.md)를 참고하세요.

Unity 2022.3 이상에서 쓰는 독립 로컬 저장 패키지입니다. UI와 Timer 패키지에 의존하지 않습니다.

## 책임

- 저장 파일에 양수 버전과 payload, SHA-256 무결성 값을 기록합니다.
- 현재 버전까지 `ISaveMigration`을 한 단계씩 적용하고, 최종 데이터를 게임의 검증 함수로 확인합니다.
- 같은 디렉터리의 임시 파일을 디스크에 flush한 뒤 처음 저장할 때는 이동하고, 기존 파일은 `File.Replace`로 교체합니다. 정상 교체 시 이전 파일을 `.bak`으로 보관합니다.
- 기본 파일이 손상되면 검증 가능한 백업을 읽습니다. 이 상태에서 다시 저장할 때는 유효한 백업을 손상된 기본 파일로 덮어쓰지 않습니다.
- 더 높은 버전의 기본 파일을 발견하면 이전 버전 앱이 이를 덮어쓰지 않도록 저장을 거부합니다.

공항의 시설 레벨, 업그레이드 분기, 금액 검증, V1→V2 변환 내용은 게임 프로젝트가 소유합니다. 해시는 우발적 손상을 탐지하기 위한 것이며 변조 방지나 암호화 수단이 아닙니다.

## 사용

```csharp
string path = Path.Combine(Application.persistentDataPath, "airport-save.json");
ISaveMigration[] migrations = { new FacilityV1ToV2Migration() };
bool isReady = VersionedSaveStore<AirportSaveV2>.TryCreate(
    path, 2, new UnityJsonSaveCodec<AirportSaveV2>(), IsValidAirportSave,
    migrations, out VersionedSaveStore<AirportSaveV2> saves);

if (isReady && saves.TryLoad(out AirportSaveV2 data, out bool wasRecovered, out bool wasMigrated))
{
    // data로 게임 상태를 구성합니다.
    // wasRecovered 또는 wasMigrated이면 적절한 시점에 saves.TrySave(data)를 호출합니다.
}
```

`TryLoad`의 `false`는 새 설치로 파일이 없는 경우와 복구 불가능한 실패를 모두 포함합니다. 게임에서는 새 게임 생성 전에 파일 존재 여부나 별도 사용자 흐름을 확인하세요. 저장 실패 시 기존 파일을 성공으로 처리하지 말고 호출자에게 알려야 합니다.

`UnityJsonSaveCodec<T>`는 Unity `JsonUtility`의 직렬화 규칙을 따릅니다. Dictionary·다형성·property 중심 데이터 등은 프로젝트의 `ISaveCodec<T>` 구현을 사용하세요. Migration은 이전 payload를 받아 다음 버전 payload를 반환하며, 누락된 단계나 최종 검증 실패는 로드를 중단합니다. 저장 모델의 새 enum 값은 문자열로 저장하는 편이 변경에 안전합니다.

## 확인할 환경

원자적 교체는 파일 시스템과 플랫폼의 `File.Replace` 지원에 의존합니다. 지원되지 않는 환경에서는 기존 파일을 유지하고 `TrySave`가 실패합니다. 배포 대상 Android 기기와 IL2CPP 빌드에서 저장·백업·강제 종료 후 복구를 별도로 확인하세요. 새 프로젝트에는 `Package Manager > Add package from disk...`에서 이 폴더의 `package.json`을 선택합니다.
