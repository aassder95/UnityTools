# view.py

# View: 사용자 인터페이스를 관리하며 사용자와의 상호작용을 처리합니다.
import tkinter as tk
from tkinter import filedialog, messagebox

class View:
    def __init__(self, root, view_model):
        self.root = root
        self.view_model = view_model
        self.root.title("CSV to JSON 변환기")
        self._create_widgets()

    def _create_widgets(self):
        """GUI 위젯을 생성합니다."""
        frame = tk.Frame(self.root, padx=10, pady=10)
        frame.pack()

        tk.Label(frame, text="CSV 파일:").grid(row=0, column=0, padx=5, pady=5, sticky="e")
        self.csv_entry = tk.Entry(frame, width=40)
        self.csv_entry.grid(row=0, column=1, padx=5, pady=5)
        tk.Button(frame, text="찾아보기", command=self._select_csv_file).grid(row=0, column=2, padx=5, pady=5)

        tk.Label(frame, text="저장 경로:").grid(row=1, column=0, padx=5, pady=5, sticky="e")
        self.json_entry = tk.Entry(frame, width=40)
        self.json_entry.grid(row=1, column=1, padx=5, pady=5)
        tk.Button(frame, text="경로 설정", command=self._select_output_file).grid(row=1, column=2, padx=5, pady=5)

        self.skip_underscore_var = tk.BooleanVar()
        self.skip_last_header_var = tk.BooleanVar()
        self.pretty_print_var = tk.BooleanVar()

        tk.Checkbutton(frame, text="'_'로 시작하는 헤더 무시", variable=self.skip_underscore_var).grid(row=2, column=0, padx=5, pady=5, sticky="w")
        tk.Checkbutton(frame, text="마지막 헤더 무시", variable=self.skip_last_header_var).grid(row=2, column=1, padx=5, pady=5, sticky="w")
        tk.Checkbutton(frame, text="프리티 프린트", variable=self.pretty_print_var).grid(row=2, column=2, padx=5, pady=5, sticky="w")

        tk.Button(frame, text="변환", command=self._convert).grid(row=3, column=0, columnspan=3, pady=10)

    def _select_csv_file(self):
        """CSV 파일을 선택하도록 사용자에게 요청합니다."""
        file_path = filedialog.askopenfilename(filetypes=[("CSV 파일", "*.csv"), ("모든 파일", "*.*")])
        if file_path:
            self.csv_entry.delete(0, tk.END)
            self.csv_entry.insert(0, file_path)

    def _select_output_file(self):
        """JSON 저장 경로를 선택하도록 사용자에게 요청합니다."""
        file_path = filedialog.asksaveasfilename(defaultextension=".txt", filetypes=[("텍스트 파일", "*.txt")])
        if file_path:
            self.json_entry.delete(0, tk.END)
            self.json_entry.insert(0, file_path)

    def _convert(self):
        """CSV를 JSON으로 변환합니다."""
        self.view_model.set_paths(self.csv_entry.get(), self.json_entry.get())
        self.view_model.set_options(
            skip_underscore=self.skip_underscore_var.get(),
            skip_last_header=self.skip_last_header_var.get(),
            pretty_print=self.pretty_print_var.get()
        )

        if not self.view_model.is_valid_paths():
            messagebox.showerror("오류", "올바른 파일 경로를 설정하세요.")
            return

        load_result = self.view_model.load_csv()
        if load_result:
            messagebox.showerror("오류", load_result)
            return

        save_result = self.view_model.save_json()
        if "오류" in save_result:
            messagebox.showerror("오류", save_result)
        else:
            messagebox.showinfo("완료", save_result)
