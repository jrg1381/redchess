SET JAVA="c:\Program Files (x86)\Java\jre7\bin\java.exe"
SET PROJECT=%~1
SET SOLUTION=%~2

REM del /s/q "%PROJECT%Parser\*"
%JAVA% -jar "%SOLUTION%antlr\antlr-3.5.1-complete.jar" -make -report -o "%PROJECT%Parser" "%PROJECT%Pgn.g" 2>errors.txt
type errors.txt

