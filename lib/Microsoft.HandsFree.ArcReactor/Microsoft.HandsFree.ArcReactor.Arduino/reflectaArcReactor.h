#pragma once

#define STATUS_LED_PIN 13
#define LED_PIN 11
#define LED_LENGTH 16

typedef enum State : byte
{
  STATE_OFF,
  STATE_TYPING,
  STATE_TALKING,
  STATE_EMERGENCY,
  STATE_ATTENDANT,
  STATE_HANDRAISE,
  STATE_SMILE,
  STATE_IDLE,
  STATE_LEFT,
  STATE_RIGHT,
  STATE_UP,
  STATE_DOWN,
  MAX_STATES
};

namespace reflectaArcReactor
{
  // basic methods   
  void setup();
  void loop();
  bool readKeepAlive();

  // interface methods
  void getLedState();
  void setLedState();

  // internal methods
  void setLedStateInternal(byte newState);
};

