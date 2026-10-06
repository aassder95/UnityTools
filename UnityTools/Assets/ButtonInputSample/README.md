# Button Input Sample

Package Manager에서 이 샘플을 Import하고 `ButtonInputSample.unity`를 열어 Play합니다.

- **SINGLE**: 눌림 스케일 연출과 일반 클릭을 확인합니다.
- **HOLD TO REPEAT**: 짧게 누르면 한 번, 길게 누르면 대기 후 점점 빠르게 클릭합니다. 떼거나 밖으로 이동하면 멈춥니다.
- **TOGGLE REPEAT**: 반복 버튼의 interactable을 전환합니다.
- **PAUSE / RESUME**: Time.timeScale을 0으로 바꿔도 UI 입력과 연출이 계속되는지 확인합니다. 샘플이 비활성화되면 이전 time scale을 복원합니다.

`UiRepeatButton`의 Inspector에서 Initial Delay Sec, Interval Sec, Min Interval Sec, Acceleration을 조정합니다. 반복은 기존 Button.onClick에 연결하며 길게 누른 뒤의 release click은 추가 호출하지 않습니다. 키보드/게임패드 Submit은 한 번 클릭합니다. 스케일 연출은 포인터 입력에만 적용합니다.

`Tools > UnityTools > Create Button Input Sample`은 Assets/ButtonInputSample의 샘플 장면을 다시 생성합니다. 기존 샘플을 수정했다면 다른 경로에 보관하세요. 기본 샘플은 StandaloneInputModule을 사용합니다. Input System 전용 프로젝트에서는 EventSystem의 입력 모듈을 프로젝트 설정에 맞게 교체합니다.
