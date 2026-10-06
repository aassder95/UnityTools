# Benchmark 측정 검수

## 범위

Benchmark 0.1.0의 프레임 수집, 워밍업, 통계, 중단·재시작, managed 할당 경로를 검수했습니다. 공개 API·결과 property·CSV 열·package version·기존 meta GUID·샘플 serialized field는 유지합니다. A/B 반복 실행과 1.0 릴리스는 후속 범위입니다.

## 확인한 경로와 개선

`BenchmarkSampler.TryCaptureFrame`은 현재 프레임의 unscaledDeltaTime과 recorder의 완료된 이전 프레임 값을 수집합니다. recorder가 준비되기 전에는 샘플을 추가하지 않으며 같은 frameCount는 다시 수집하지 않습니다. [Unity LastValue 계약](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorder.LastValue.html)을 따릅니다. FixedUpdate나 한 프레임의 여러 위치에서 호출하지 않고 매 프레임 같은 위치에서 호출합니다.

선택한 마커가 실행되지 않으면 Count 검사가 전체 수집을 멈출 가능성을 먼저 검토했습니다. 실제 Unity 2022.3.62f3 Play Mode에서 등록된 미실행 마커도 0 표본으로 완료되는 것을 확인했습니다. 원래 수집 분기를 유지하고 회귀 테스트를 추가했습니다. recorder의 기본 옵션은 프레임 안의 마커 호출을 합산합니다. [Unity recorder 옵션](https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Unity.Profiling.ProfilerRecorderOptions.html)을 참고하세요.

결과 생성은 완료된 세 샘플 배열을 Clone하고 정렬해 프레임 수에 비례하는 추가 메모리를 만들었습니다. 완료 후 AddFrame을 거절하므로 배열을 그대로 정렬할 수 있습니다. 이를 통해 배열 복제 세 건을 제거했습니다. 마커가 없을 때는 기존의 두 배열 복제를 제거합니다. 결과 객체는 기존 API대로 새로 생성합니다. 반복 결과 조회의 통계값도 유지합니다.

배열 복제만 제거한 단계에서는 큰 표본의 결과 생성에 여전히 4건이 잡혔습니다. 별도 임시 프로젝트 진단에서 기본 Array.Sort<double>는 정렬당 1건을 할당했습니다. 비교용 Comparison<double>를 미리 생성해 Array.Sort의 comparison overload에 전달하고 재사용합니다. 두 Unity 버전에서 작은·큰 표본의 결과 생성이 객체 1건만 할당하는 것을 검증합니다. object 비교를 쓰는 Array.Sort(Array)로 우회하지 않습니다.

UTC 조회는 결과가 없는 모든 수집 프레임에도 실행됐습니다. IsComplete 확인을 먼저 하여 완료 시점에만 조회하도록 변경했습니다.

## 할당 검증 방법

이 환경의 Unity 2022.3 Mono에서 `GC.GetAllocatedBytesForCurrentThread()`는 4096 bytes 배열을 생성한 양성 대조군에서도 0을 반환했습니다. 해당 값을 할당 검증 근거로 사용하지 않습니다.

테스트는 Unity GC.Alloc recorder를 현재 thread에 한정해 사용하고 측정 구간 직전에 Reset/Start, 직후 Stop합니다. assertion과 coroutine yield는 측정 밖에 둡니다. 별도 양성 대조군이 실제 배열 할당을 검출해야 하며, 미완료 세션과 실제 sampler의 수집 구간은 0건이어야 합니다. 결과 생성은 정렬 경로의 초기화를 마친 뒤 작은·큰 표본 모두 결과 객체 1건만 할당해야 합니다. 초기화·최초 JIT·모든 플랫폼의 비용까지 0이라는 주장으로 확대하지 않습니다.

수정 전 테스트에서는 결과 생성 할당이 기대한 1건 대신 4건으로 실패했습니다. 다른 13개 테스트는 통과했습니다. 수정 후 결과는 아래 검증 기록으로 구분합니다.

## 회귀 검증

- 역순 표본 1/20/100개의 nearest-rank P95와 반복 결과 조회
- 워밍업 통계 제외, 잘못된 프레임이 워밍업·샘플을 소비하지 않는지
- 미완료 세션·sampler의 수집 할당과 완료 결과의 할당 수
- 유효한 미실행 마커의 0 결과와 실제 마커 수집
- 중복 시작·동일 프레임 중복 수집 거절, 취소 후 재시작, 반복 Release
- 기존 CSV 헤더·escape 동작

2026-10-01에 Local UPM 참조로 검증했습니다.

| Unity | 코어 Play Mode | UI Performance Lab Play Mode | Windows Mono build |
| --- | --- | --- | --- |
| 2022.3.62f3 | 14/14 통과 | 21/21 통과 | 코어·실험실 모두 성공 |
| 6000.3.20f1 | 14/14 통과 | 21/21 통과 | 코어·실험실 모두 성공 |

코어는 uGUI 없이 설치했습니다. 실험실은 UnityTools UI를 함께 설치하고 Package Manager Sample API로 Import한 뒤 장면 검사·테스트·빌드를 실행했습니다. 두 버전 모두 양성 대조군의 할당 검출, 미완료 수집 0건, 완료 결과 1건 검사가 통과했습니다. 정적 UPM 검사 4개와 UI·실험실 샘플 미러 검사도 통과했습니다.

증거 경로는 다음과 같습니다. 각 summary.json 및 benchmark/ui-lab 하위 results.xml·Editor 로그·Build 결과를 보존했습니다.

- `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-2022.3.62f3-a337e81b019343d593fc6bb51fbf2a03`
- `C:/Users/search/AppData/Local/Temp/UnityTools-Compatibility-6000.3.20f1-a3a0ec167e8e40509aac81a80e41f0a3`

공개 Git tag 설치 검증과 구분합니다. Android/iOS·IL2CPP·WebGL·Release Player 카운터·실제 기기 성능 개선율은 이번 검증 범위에 포함하지 않습니다.
