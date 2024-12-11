# model.py

# Model: CSV 데이터를 필터링하고 JSON으로 저장하는 역할을 담당합니다.
import csv
import json

class Model:
    def __init__(self):
        self.filtered_data = []

    def load_csv(self, csv_path, skip_last=False, skip_underscore=False):
        """CSV 파일을 로드하고 데이터를 필터링합니다."""
        try:
            with open(csv_path, 'r', encoding='utf-8-sig') as csv_file:
                reader = csv.DictReader(csv_file)
                headers = reader.fieldnames

                if skip_last and headers:
                    headers = headers[:-1]
                    reader.fieldnames = headers

                self.filtered_data = [
                    {k: v for k, v in row.items() if k is not None and (not skip_underscore or not k.startswith('_'))}
                    for row in reader
                ]
        except FileNotFoundError:
            raise FileNotFoundError(f"CSV 파일을 찾을 수 없습니다: {csv_path}")
        except Exception as e:
            raise Exception(f"CSV 파일 읽기 중 오류 발생: {e}")

    def save_json(self, json_path, pretty_print=False):
        """필터링된 데이터를 JSON 파일로 저장합니다."""
        try:
            with open(json_path, 'w', encoding='utf-8') as json_file:
                json.dump(
                    self.filtered_data,
                    json_file,
                    ensure_ascii=False,
                    indent=4 if pretty_print else None
                )
        except Exception as e:
            raise Exception(f"JSON 파일 저장 중 오류 발생: {e}")
