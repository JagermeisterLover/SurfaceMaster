# SurfaceMaster
SurfaceMaster is a tool that calcualtes data for various aspheric surfaces (used in optical design). It can calculate r,z data for 5 surface types, graph it, convert surface equation to any other type.
Supported surface types:
1) Even Asphere (up to 20th coefficient)
2) Odd Asphere (up to 20th coefficient)
3) OPAL-PC universal Z
4) OPAL-PC universal U
5) Russian polynomial equation (ОСТ 3-4918-93) 

SurfaceMaster is a windows forms app that requires .NET 8. SurfaceMaster requires compiled EquationFitter.exe in the program folder. Equation fitter is written on python and needs to be compiled to .exe (using pyinstaller --onefile equationfitter.py).
Equationfitter.py is located in OpticsEquationFitter repository https://github.com/JagermeisterLover/OpticsEquationFitter.
