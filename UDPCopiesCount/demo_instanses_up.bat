echo off

:parseArgs
call:getArgWithValue "-nodes_count" "NODES_COUNT" "%~1" "%~2" && shift && shift && goto :parseArgs

:: Очистить логи
ECHO "Clear log folder"
del /s /q "./demo-logs/"


:: Запустить трудяг - писателей
SET count= %NODES_COUNT%-1
ECHO "Start simple nodes:"
ECHO %count%

for /l %x in (1, 1, count) do (
    start "./publish/UDPCopiesCount.exe"
) %x
:: Запустить слушателя


:: =====================================================================
:: This function sets a variable from a cli arg with value
:: 1 cli argument name
:: 2 variable name
:: 3 current Argument Name
:: 4 current Argument Value
:getArgWithValue
if "%~3"=="%~1" (
  if "%~4"=="" (
    REM unset the variable if value is not provided
    set "%~2="
    exit /B 1
  )
  set "%~2=%~4"
  exit /B 0
)
exit /B 1
goto:eof



:: =====================================================================
:: This function sets a variable to value "TRUE" from a cli "flag" argument
:: 1 cli argument name
:: 2 variable name
:: 3 current Argument Name
:getArgFlag
if "%~3"=="%~1" (
  set "%~2=TRUE"
  exit /B 0
)
exit /B 1
goto:eof