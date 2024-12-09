import tkinter as tk
from tkinter import filedialog, messagebox
import csv
import json
import os

def select_csv_file():
    file_path = filedialog.askopenfilename(
        title="CSV 파일 선택",
        filetypes=[("CSV files", "*.csv"), ("All Files", "*.*")]
    )
    if file_path:
        csv_path_var.set(file_path)

def select_output_file():
    file_path = filedialog.asksaveasfilename(
        title="저장할 파일 경로 선택",
        defaultextension=".txt",
        filetypes=[("Text Files", "*.txt"), ("All Files", "*.*")]
    )
    if file_path:
        json_path_var.set(file_path)

def convert_csv_to_json():
    csv_file_path = csv_path_var.get()
    json_file_path = json_path_var.get()

    if not csv_file_path or not os.path.exists(csv_file_path):
        messagebox.showerror("에러", "유효한 CSV 파일을 선택해주세요.")
        return
    if not json_file_path:
        messagebox.showerror("에러", "저장할 TXT 파일 경로를 설정해주세요.")
        return

    skip_underscore = skip_underscore_var.get()  # '_' 시작 헤더 무시 여부
    skip_last = last_header_var.get()            # 마지막 헤더 무시 여부
    pretty_print = pretty_print_var.get()        # 프리티 프린트 여부

    try:
        with open(csv_file_path, 'r', encoding='utf-8-sig') as csv_file:
            reader = csv.DictReader(csv_file)
            headers = reader.fieldnames

            # 마지막 헤더 무시 옵션 적용
            if skip_last and headers:
                headers = headers[:-1]
                reader.fieldnames = headers

            filtered_data = []
            for row in reader:
                # None 키 제거(헤더보다 많은 컬럼 방어)
                row = {k: v for k, v in row.items() if k is not None}

                # '_' 시작 헤더 무시 옵션 적용
                if skip_underscore:
                    row = {k: v for k, v in row.items() if not k.startswith('_')}

                filtered_data.append(row)

        # JSON 쓰기 옵션 설정
        dump_options = {'ensure_ascii': False}
        if pretty_print:
            dump_options['indent'] = 4

        with open(json_file_path, 'w', encoding='utf-8') as json_file:
            json.dump(filtered_data, json_file, **dump_options)
        
        messagebox.showinfo("완료", "CSV를 JSON 형식 TXT 파일로 변환 완료!")
    except Exception as e:
        messagebox.showerror("에러", f"변환 중 오류 발생: {e}")

# 메인 윈도우 생성
root = tk.Tk()
root.title("CSV to JSON TXT 변환기")

# 변수 바인딩
csv_path_var = tk.StringVar()
json_path_var = tk.StringVar()
skip_underscore_var = tk.IntVar(value=0)  # '_' 시작 헤더 무시
last_header_var = tk.IntVar(value=0)      # 마지막 헤더 무시
pretty_print_var = tk.IntVar(value=0)     # 프리티 프린트

frame = tk.Frame(root, padx=10, pady=10)
frame.pack()

# CSV 파일 선택
csv_label = tk.Label(frame, text="CSV 파일:")
csv_label.grid(row=0, column=0, sticky="e", padx=5, pady=5)

csv_entry = tk.Entry(frame, textvariable=csv_path_var, width=40)
csv_entry.grid(row=0, column=1, padx=5, pady=5)

csv_button = tk.Button(frame, text="선택", command=select_csv_file)
csv_button.grid(row=0, column=2, padx=5, pady=5)

# JSON(txt) 파일 경로 설정
json_label = tk.Label(frame, text="저장할 TXT 파일 경로:")
json_label.grid(row=1, column=0, sticky="e", padx=5, pady=5)

json_entry = tk.Entry(frame, textvariable=json_path_var, width=40)
json_entry.grid(row=1, column=1, padx=5, pady=5)

json_button = tk.Button(frame, text="경로 설정", command=select_output_file)
json_button.grid(row=1, column=2, padx=5, pady=5)

# 체크박스 가로 배치
skip_checkbox = tk.Checkbutton(frame, text="'_' 시작 헤더 무시", variable=skip_underscore_var)
skip_checkbox.grid(row=2, column=0, padx=5, pady=5, sticky="w")

last_checkbox = tk.Checkbutton(frame, text="마지막 헤더 무시", variable=last_header_var)
last_checkbox.grid(row=2, column=1, padx=5, pady=5, sticky="w")

pretty_checkbox = tk.Checkbutton(frame, text="프리티프린트", variable=pretty_print_var)
pretty_checkbox.grid(row=2, column=2, padx=5, pady=5, sticky="w")

# 변환 버튼
convert_button = tk.Button(frame, text="변환하기", command=convert_csv_to_json)
convert_button.grid(row=3, column=0, columnspan=3, pady=10)

root.mainloop()
