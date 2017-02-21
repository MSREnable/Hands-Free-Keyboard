#include "Version.h"
#include <Arduino.h>
#include <Reflecta.h>

#include "ReflectaArcReactor.h"

bool readKeepAlive()
{
  reflectaHeartbeat::push(reflectaHeartbeat::alive());
  
  return true;
}

// Initialize everything and prepare to start
void setup()
{
  // immediately disable watchdog timer so setup will not get interrupted
  wdt_disable();

  reflectaFunctions::setFirmwareVersion(FIRMWARE_VERSION);
  
  reflectaFrames::setup(57200); 
  reflectaFunctions::setup(); 
  reflectaArduinoCore::setup(); 
  reflectaHeartbeat::setup();

  // 1FPS
  reflectaFunctions::push16(1); 
  reflectaHeartbeat::setFrameRate(); 

  reflectaHeartbeat::bind(readKeepAlive);
  reflectaHeartbeat::bind(reflectaArcReactor::readKeepAlive);

  reflectaArcReactor::setup();
}

// Main loop
void loop()
{
  // Turn on the watchdog timer which will cause a system reset on hang
  // Need to do this here as the Arduino code seems to be resetting it after setup() is completed
  // so if this is done in setup() it gets turned off again before the loop() is run
  wdt_enable(WDTO_500MS);
  
  reflectaFrames::loop();
  reflectaHeartbeat::loop();
  
  reflectaArcReactor::loop();
}



