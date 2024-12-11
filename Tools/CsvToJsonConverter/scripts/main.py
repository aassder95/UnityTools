# main.py

# main: 프로그램의 진입점.
from tkinter import Tk
from mvvm.model import Model
from mvvm.view import View
from mvvm.viewmodel import ViewModel

def main():
    root = Tk()
    model = Model()
    view_model = ViewModel(model)
    view = View(root, view_model)
    root.mainloop()

if __name__ == "__main__":
    main()
