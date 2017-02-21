#include <Reflecta.h>
#include "NeoPattern.h"
#include "ReflectaArcReactor.h"

namespace reflectaArcReactor
{
  State currentLedState = STATE_OFF;

  // NeoPatterns for the two rings and the stick
  //  as well as some completion routines
  NeoPattern Ring1(LED_LENGTH, LED_PIN, NEO_GRB + NEO_KHZ800, NULL);

  // basic methods
  void setup()
  {
    reflectaFunctions::bind("arcr1", setLedState);
    reflectaFunctions::bind("arcr1", getLedState);

    Ring1.begin(); // Initialize the ring
    setLedStateInternal(STATE_OFF);

    pinMode(STATUS_LED_PIN, OUTPUT);
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
      digitalWrite(STATUS_LED_PIN, HIGH);
    } else {
      // Status LED off
      digitalWrite(STATUS_LED_PIN, LOW);

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
    }
  }   
};

