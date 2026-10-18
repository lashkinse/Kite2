@echo off
rem Запуск Kite2 в тестовом режиме: страницы рисует программа, сканер не нужен.
rem Ключ --test включает тестовый режим, --ask добавляет вопрос <Сканировать ещё?>
rem после первой серии. Остальные ключи из командной строки передаются программе.

setlocal

set "root=%~dp0"
set "program=%root%src\Kite2.App\bin\x64\Debug\net48\Kite2.exe"

rem Собранной копии нет - сначала соберите решение, иначе запускать нечего.
if not exist "%program%" set "program=%root%src\Kite2.App\bin\x64\Release\net48\Kite2.exe"

if exist "%program%" goto run

echo Kite2 не собран. Соберите решение командой:
echo     dotnet build "%root%Kite2.sln" -c Debug
pause
exit /b 1

:run
start "" "%program%" --test %*
exit /b 0
