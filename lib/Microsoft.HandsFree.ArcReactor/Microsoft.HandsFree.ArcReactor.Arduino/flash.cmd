SETLOCAL
set BOARDTYPE=ArduinoMicro

for /F "tokens=1-2 delims=," %%A in ('..\..\..\external\ReflectaCli\ReflectaCli.exe') do (
set oldfirmware=%%A 
set comport=%%B
)

for %%f in (*.hex) do set firmware=%%f
set newfirmware=%firmware:~0,-4%
if [%newfirmware%] == [%oldfirmware%] (
echo Firmware up to date
) else (
echo Updating firmware to %newfirmware%
IF "%BOARDTYPE%"=="ArduinoMicro" (
..\..\..\external\avrdude\avrdude -C..\..\..\external\avrdude\avrdude.conf -v -patmega32u4 -cavr109 -P%comport% -b57600 -D -Uflash:w:%firmware%:i
)
IF "%BOARDTYPE%"=="Teensy2" (
set TY_EXPERIMENTAL_BOARDS=true
..\..\..\external\TyQt-0.7.6-win64\tyc.exe upload %firmware%
)
)