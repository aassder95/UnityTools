# viewModel.py

# ViewModel: View와 Model 간의 연결을 관리하고 데이터 바인딩을 처리합니다.
class ViewModel:
    def __init__(self, model):
        self.model = model
        self.csv_path = None
        self.json_path = None
        self.skip_underscore = False
        self.skip_last_header = False
        self.pretty_print = False

    def set_paths(self, csv_path, json_path):
        """CSV 파일 경로와 JSON 저장 경로를 설정합니다."""
        self.csv_path = csv_path
        self.json_path = json_path

    def set_options(self, skip_underscore, skip_last_header, pretty_print):
        """CSV 처리 옵션을 설정합니다."""
        self.skip_underscore = skip_underscore
        self.skip_last_header = skip_last_header
        self.pretty_print = pretty_print

    def load_csv(self):
        """Model에 CSV 데이터 로드를 요청합니다."""
        try:
            self.model.load_csv(
                csv_path=self.csv_path,
                skip_last=self.skip_last_header,
                skip_underscore=self.skip_underscore
            )
            return None
        except Exception as e:
            return f"로드 중 오류 발생: {e}"

    def save_json(self):
        """Model에 JSON 데이터 저장을 요청합니다."""
        try:
            self.model.save_json(
                json_path=self.json_path,
                pretty_print=self.pretty_print
            )
            return "Completed"
        except Exception as e:
            return f"저장 중 오류 발생: {e}"

    def is_valid_paths(self):
        """경로 유효성 검사를 수행합니다."""
        return bool(self.csv_path) and bool(self.json_path)
