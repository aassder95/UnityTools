# Benchmark 1.0 API와 CSV 계약

## 수집

`BenchmarkSampler.TryStart(scenarioId, agentCnt, seed, warmupFrames, sampleFrames, markerName = null)`은 실행 중·잘못된 설정·미지원 recorder일 때 false입니다. 실패 시 실행을 시작하지 않습니다. optional marker는 Scripts category에 사전 등록합니다.

`TryCaptureFrame(out BenchmarkResult)`은 시작 프레임과 같은 프레임 재호출을 제외하고, 완료된 이전 프레임의 recorder 값과 unscaledDeltaTime을 수집합니다. false는 대기·미실행·유효하지 않은 프레임에도 반환되므로 오류 표시가 아닙니다. 완료한 호출만 true이며 sampler가 해제됩니다. `IsRunning`으로 실행 상태를 확인하고 중단·파괴 시 `Release()`합니다.

`BenchmarkSession.TryCreate(..., out session)`으로 Unity recorder 없이 표본을 입력할 수 있습니다. `AddFrame(frameIntervalMs, mainThreadNs, gcBytes, memoryBytes, markerNs)`은 유효 프레임에서 true이며 워밍업은 SampleCnt에 포함하지 않습니다. 프레임 간격·Main Thread는 양수, 다른 값은 음수가 아니어야 합니다. 완료한 세션은 추가 입력을 거절합니다. `TryGetResult(recordedUtc, out result)`에는 UTC 시각을 전달합니다. 완료 후 반복 호출은 각각 결과 객체를 생성합니다.

P95는 표본을 정렬한 ceil(N × 0.95) − 1 인덱스입니다. FPS는 1000 / 평균 프레임 간격이며 개별 FPS의 평균이 아닙니다. 메모리는 프로세스 측정값이고 특정 객체의 점유량이 아닙니다. 마커 미지정은 이름 공백, 마커 평균·P95는 0입니다.

## 코어 CSV v1

`BenchmarkCsv.HEADER`는 아래 21열 순서이며 1.x에서 유지합니다. `ToRow(null)`은 빈 문자열입니다. 문자열의 쉼표·따옴표·개행은 CSV quoting하고 숫자는 invariant culture, UTC는 round-trip O 형식입니다. UTF-8 BOM 없이 기록합니다.

```text
scenario_id,agent_cnt,seed,warmup_frames,sample_frames,marker_name,unity_version,device_model,platform,recorded_utc,mean_frame_ms,p95_frame_ms,max_frame_ms,fps_from_mean_frame,mean_main_thread_ms,p95_main_thread_ms,mean_gc_bytes,total_gc_bytes,peak_memory_bytes,mean_marker_ms,p95_marker_ms
```

`TryAppend(path, result)`는 null·빈 경로 및 처리하는 파일 I/O 실패에서 false입니다. 새 파일 또는 길이 0 파일에만 헤더를 기록합니다. 기존 파일 헤더를 검증하지 않으므로 호출자가 이 계약의 파일만 전달해야 합니다. 디렉터리 생성·동시 writer 직렬화·중복 제거·transaction·부분 쓰기 복구는 제공하지 않습니다. 측정이 끝난 뒤 호출하세요.

UI 샘플의 33열 단일 비교 CSV와 52열 series CSV는 코어 CSV와 별도 계약입니다. 샘플 README에 열 의미와 중복 export 처리가 있습니다. 파일명을 구분하세요.

## 버전 및 환경

공개 API·열 제거와 재정렬은 major 변경 대상입니다. package 버전은 CSV schema 및 사용자 scenario 버전과 별개입니다. Development Player에서 측정하고 같은 기기·빌드·화면·품질·시드·샘플 수를 비교합니다. Editor와 Player 결과를 섞지 않습니다. 측정 및 정렬·CSV 비용 범위는 measurement.md를 참고하세요.
