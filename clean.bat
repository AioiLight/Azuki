@echo off

cd Azuki
call clean.bat
cd ..

cd Ann
call clean.bat
cd ..

cd doc
call clean.bat
cd ..

del /ah     *.suo           2> NUL
del         package\log.txt 2> NUL
rmdir /s /q package\zip     2> NUL
