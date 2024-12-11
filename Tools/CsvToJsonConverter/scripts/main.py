# main.py

# main: 프로그램의 진입점.
from mvvm.model import Model
from mvvm.view import View
from mvvm.viewmodel import ViewModel

if __name__ == "__main__":
    from tkinter import Tk

    root = Tk()
    model = Model()
    view_model = ViewModel(model)
    view = View(root, view_model)
    root.mainloop()
