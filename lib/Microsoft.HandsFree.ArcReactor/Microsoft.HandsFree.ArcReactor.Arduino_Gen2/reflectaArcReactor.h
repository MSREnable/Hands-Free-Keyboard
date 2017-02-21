#pragma once

enum State : byte
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
  STATE_IMAGE,
  STATE_ANIMATION,
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
  void showImage();
  void showImageColor();
  void showAnimation();
  void showPixel();
  void showPixelColor();

  // internal methods
  void setLedStateInternal(byte newState);
};

