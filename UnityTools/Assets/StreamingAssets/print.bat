@echo off

echo %PATH%
rem timeout 3 is not working, so use following for waiting
waitfor SomethingThatIsNeverHappening /t 3 2>NUL
rem ping localhost -n 4
ipconfig