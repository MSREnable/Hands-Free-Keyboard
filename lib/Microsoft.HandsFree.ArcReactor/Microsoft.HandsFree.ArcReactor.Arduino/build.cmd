SETLOCAL
set FIRMWARE_NAME=arcreactor
set BOARDTYPE=ArduinoMicro
set libraries="Adafruit NeoPixel"

for %%* in (.) do set CurrDirName=%%~nx*

REM http://ss64.com/nt/syntax-getdate.html
FOR /F "skip=1 tokens=1-6" %%G IN ('WMIC Path Win32_LocalTime Get Day^,Hour^,Minute^,Month^,Second^,Year /Format:table') DO (
   IF "%%~L"=="" GOTO s_done
      SET _yyyy=%%L
      SET _mm=00%%J
      SET _dd=00%%G
      SET _hour=00%%H
      SET _minute=00%%I
      SET _second=00%%K
)
:s_done

SET _mm=%_mm:~-2%
SET _dd=%_dd:~-2%
SET _hour=%_hour:~-2%
SET _minute=%_minute:~-2%

del *.hex
rd /S /Q bin
mkdir bin
for /f %%x in ('wmic os get localdatetime /format:list ^| findstr "="') do set %%x
set FIRMWARE_VERSION=%FIRMWARE_NAME%-%_yyyy%.%_mm%.%_dd%.%_hour%%_minute%
echo const char* FIRMWARE_VERSION = "%FIRMWARE_VERSION%"; > Version.h
IF NOT %libraries%=="" (
"%ProgramFiles(x86)%\Arduino\arduino_debug.exe" --install-library %libraries%
)
REM "%ProgramFiles(x86)%\Arduino\arduino_debug.exe" --install-library "Reflecta"
REM "%ProgramFiles(x86)%\Arduino\arduino_debug.exe" --install-boards 
IF "%BOARDTYPE%"=="ArduinoMicro" (
"%ProgramFiles(x86)%\Arduino\arduino_debug.exe" --verify --board arduino:avr:micro --pref build.path=bin %CurrDirName%.ino
)
IF "%BOARDTYPE%"=="Teensy2" (
"%ProgramFiles(x86)%\Arduino\arduino_debug.exe" --verify --board teensy:avr:teensy2:usb=serial,speed=16,keys=en-us --pref build.path=bin %CurrDirName%.ino
)
copy bin\%CurrDirName%.ino.hex %FIRMWARE_VERSION%.hex
rd /S /Q bin

(
echo namespace %CurrDirName%
echo {
echo     static class Version
echo     {
echo         public const string CurrentVersion = "%FIRMWARE_VERSION%";
echo     }
echo }

)>Version.cs
