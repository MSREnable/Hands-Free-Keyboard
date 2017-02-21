#include <Reflecta.h>
#include "NeoPattern.h"
#include "ReflectaArcReactor.h"
#include "Animations.h"

namespace reflectaArcReactor
{
  const int dbg_led0 = PIN_D0;
  const int dbg_led1 = PIN_D1;
  const int dbg_led2 = PIN_D2;
  const int dbg_led3 = PIN_D3;
  const int dbg_led4 = PIN_D4;
  const int dbg_led5 = PIN_D5;
  const int dbg_led6 = PIN_D6;
  const int dbg_led7 = PIN_D7;
  
  const int dbg_led_period = 50;
  
  const int neopix_pwr_en_n = PIN_B0;
  const int neopix_data = PIN_B7;
  const int neopix_length = 35;
  
  State currentLedState = STATE_OFF;

  // NeoPatterns for the two rings and the stick
  //  as well as some completion routines
  NeoPattern Ring1(neopix_length, neopix_data, NEO_GRB + NEO_KHZ800, NULL);

  // basic methods
  void setup()
  {
    reflectaFunctions::bind("arcr1", setLedState);
    reflectaFunctions::bind("arcr1", getLedState);
    reflectaFunctions::bind("arcr1", showImage);
    reflectaFunctions::bind("arcr1", showImageColor);
    reflectaFunctions::bind("arcr1", showAnimation);
    reflectaFunctions::bind("arcr1", showPixel);
    reflectaFunctions::bind("arcr1", showPixelColor);

    // turn off power to neopixels
    digitalWrite(neopix_pwr_en_n, HIGH);
    pinMode(neopix_pwr_en_n, OUTPUT);
    digitalWrite(neopix_pwr_en_n, HIGH);
  
    // drive neopixel data line low
    digitalWrite(neopix_data, LOW);
    pinMode(neopix_data, OUTPUT);
    digitalWrite(neopix_data, LOW);

    // turn on neopixel power supply
    digitalWrite(neopix_pwr_en_n, LOW);  
    delay(100);

    Ring1.begin(); // Initialize the ring
    setLedStateInternal(STATE_OFF);

    pinMode(dbg_led0, OUTPUT);
  }

  void loop()
  {
    // Update the rings.
    Ring1.Update();
  }

  bool readKeepAlive()
  {
    if (reflectaHeartbeat::alive()) {
      // Status LED on to indicate connection
      digitalWrite(dbg_led0, HIGH);
    } else {
      // Status LED off
      digitalWrite(dbg_led0, LOW);

      // Turn off any existing visual
      reflectaArcReactor::setLedStateInternal(STATE_IDLE);
      Ring1.IdleInit(Ring1.Color(25, 0, 0));
    }
  
    return true;
  }

  // interface methods
  void getLedState()
  {
    // return state
    byte resultState[] = { currentLedState };
    reflectaFunctions::sendResponse(sizeof(resultState), resultState);
  }

  void setLedState()
  {
    // get newState from stack
    byte newState = reflectaFunctions::pop();

    setLedStateInternal(newState);

    // return state
    byte resultState[] = { currentLedState };
    reflectaFunctions::sendResponse(sizeof(resultState), resultState);
  }

  void showImage()
  {
    for (uint16_t idx = 0; idx < neopix_length; ++idx)
    {
      reflectaFunctions::pop(); // ignored
      byte red = reflectaFunctions::pop();
      byte green = reflectaFunctions::pop();
      byte blue = reflectaFunctions::pop();
      Ring1.Static_Image[idx] = ((uint32_t)(red) << 16) | ((uint32_t)(green) << 8) | (uint32_t)(blue);

      // TODO: investigate why the pop32 variants are not working
      //uint32_t c = reflectaFunctions::pop32();

      //byte red   = (uint8_t)(c >> 16);
      //byte green = (uint8_t)(c >>  8);
      //byte blue  = (uint8_t)(c >>  0);
      //Ring1.Static_Image[idx] = Ring1.Color(red, green, blue); 

      // TODO: investigate why the pop32 variants are not working
      //Ring1.Static_Image[idx] = reflectaFunctions::pop32();
    }
    
    // get newState from stack
    byte newState = STATE_IMAGE;

    // Set the static_image data
    setLedStateInternal(newState);

    // return state
    byte resultState[] = { currentLedState };
    reflectaFunctions::sendResponse(sizeof(resultState), resultState);
  }

  void showImageColor()
  {
    for (uint16_t idx = 0; idx < neopix_length; ++idx)
    {
        byte red = reflectaFunctions::pop();
        byte green = reflectaFunctions::pop();
        byte blue = reflectaFunctions::pop();
        Ring1.Static_Image[idx] = Ring1.Color(red, green, blue);
    }
    
    // get newState from stack
    byte newState = STATE_IMAGE;

    // Set the static_image data
    setLedStateInternal(newState);

    // return state
    byte resultState[] = { currentLedState };
    reflectaFunctions::sendResponse(sizeof(resultState), resultState);
  }

  void showPixel()
  {
    byte idx = reflectaFunctions::pop();

    reflectaFunctions::pop(); // ignored
    byte red = reflectaFunctions::pop();
    byte green = reflectaFunctions::pop();
    byte blue = reflectaFunctions::pop();
    Ring1.Static_Image[idx] = ((uint32_t)(red) << 16) | ((uint32_t)(green) << 8) | (uint32_t)(blue);

    // TODO: investigate why the pop32 variants are not working
    //uint32_t c = reflectaFunctions::pop32();

    //byte red   = (uint8_t)(c >> 16);
    //byte green = (uint8_t)(c >>  8);
    //byte blue  = (uint8_t)(c >>  0);
    //Ring1.Static_Image[idx] = Ring1.Color(red, green, blue); 

    // TODO: investigate why the pop32 variants are not working
    //Ring1.Static_Image[idx] = reflectaFunctions::pop32();
    
    // get newState from stack
    byte newState = STATE_IMAGE;

    // Set the static_image data
    setLedStateInternal(newState);

    // return state
    byte resultState[] = { currentLedState };
    reflectaFunctions::sendResponse(sizeof(resultState), resultState);
  }

  void showPixelColor()
  {
    byte idx = reflectaFunctions::pop();
    byte red = reflectaFunctions::pop();
    byte green = reflectaFunctions::pop();
    byte blue = reflectaFunctions::pop();
    
    Ring1.Static_Image[idx] = Ring1.Color(red, green, blue);
    
    // get newState from stack
    byte newState = STATE_IMAGE;

    // Set the static_image data
    setLedStateInternal(newState);

    // return state
    byte resultState[] = { currentLedState };
    reflectaFunctions::sendResponse(sizeof(resultState), resultState);
  }

  void showAnimation()
  {
    Ring1.currentAnimation = reflectaFunctions::pop();
    if (MAX_ANIMATIONS > Ring1.currentAnimation)
    {
      byte newState = STATE_ANIMATION;

      setLedStateInternal(newState);
    }
    else
    {
      // perhaps introduce a new STATE_ERROR
      byte newState = STATE_IDLE;

      setLedStateInternal(newState);
    }

    // return state
    byte resultState[] = { currentLedState };
    reflectaFunctions::sendResponse(sizeof(resultState), resultState);
  }

  // internal methods
  void setLedStateInternal(byte newState)
  {
    currentLedState = (State) newState;
  
    switch (currentLedState)
    {
    case STATE_OFF:
      Ring1.NoneInit();
      break;
    case STATE_TYPING:
      Ring1.EllipsisInit(Ring1.Color(0, 0, 50), Ring1.Color(0, 0, 0), 100);
      break;
    case STATE_TALKING:
      Ring1.PulsingInit(Ring1.Color(128, 0, 128), 75, true);
      break;
    case STATE_EMERGENCY:
      Ring1.TheaterChaseInit(Ring1.Color(128, 0, 0), Ring1.Color(0, 0, 0), 100);
      break;
    case STATE_ATTENDANT:
      Ring1.SolidInit(Ring1.Color(255, 153, 0));
      break;
    case STATE_HANDRAISE:
      Ring1.WedgeInit(Ring1.Color(0, 75, 0), Ring1.Color(0, 0, 0), 100, false);
      break;
    case STATE_SMILE:
      Ring1.SmileInit(Ring1.Color(0, 0, 100), Ring1.Color(255, 255, 255), Ring1.Color(0, 0, 0), 100);
      break;
    case STATE_IDLE:
      Ring1.IdleInit(Ring1.Color(0, 0, 50));
      break;
    case STATE_LEFT:
      Ring1.DirectionLeftInit(Ring1.Color(0, 0, 50), Ring1.Color(0, 0, 0));
      break;
    case STATE_RIGHT:
      Ring1.DirectionRightInit(Ring1.Color(0, 0, 50), Ring1.Color(0, 0, 0));
      break;
    case STATE_UP:
      Ring1.DirectionUpInit(Ring1.Color(0, 0, 50), Ring1.Color(0, 0, 0));
      break;
    case STATE_DOWN:
      Ring1.DirectionDownInit(Ring1.Color(0, 0, 50), Ring1.Color(0, 0, 0));
      break;
    case STATE_IMAGE:
      Ring1.ShowImageInit();
      break;
    case STATE_ANIMATION:
      Ring1.ShowAnimationInit();
      break;
    case MAX_STATES:
      //error
      break;
    }
  }   
};

