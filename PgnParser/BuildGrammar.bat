
IF DEFINED TEAMCITY_JRE (SET JAVA="%TEAMCITY_JRE%\bin\java.exe") ELSE (SET JAVA="C:\Program Files (x86)\Java\jre1.8.0_31\bin\java.exe")
SET PROJECT=%~1
SET SOLUTION=%~2

echo %JAVA%

rem del /s/q "%PROJECT%Parser\*"
%JAVA% -jar "%SOLUTION%antlr\antlr-4.5.1-complete.jar" -o "%PROJECT%Parser" "%PROJECT%Pgn.g4" 2>errors.txt
type errors.txt | find /v "warning"
EXIT /B 0

