#pragma once
#include <Adafruit_NeoPixel.h>
#include <avr/pgmspace.h>
#include "Animations.h"

// Pattern types supported:
enum pattern {
	NONE,
	RAINBOW_CYCLE,
	THEATER_CHASE,
	COLOR_WIPE,
	SCANNER,
	FADE,
	ELLIPSIS,
	PULSING,
	SOLID,
	WEDGE,
	SMILE,
	IDLE,
	DIRECTION_LEFT,
	DIRECTION_RIGHT,
	DIRECTION_UP,
	DIRECTION_DOWN,
	SHOW_IMAGE,
	SHOW_ANIMATION
};
// Patern directions supported:
enum direction { FORWARD, REVERSE };

// NeoPattern Class - derived from the Adafruit_NeoPixel class
class NeoPattern : public Adafruit_NeoPixel
{
public:

	pattern ActivePattern;    // which pattern is running
	direction Direction;      // direction to run the pattern
	bool AutoReverse;         // should the pattern autoreverse

	unsigned long Interval;   // milliseconds between updates
	unsigned long lastUpdate; // last update of position

	uint32_t Color1;          // What colors are in use
	uint32_t Color2;
	uint32_t Color3;
	uint16_t TotalSteps;      // total number of steps in the pattern
	uint16_t Index;           // current step within the pattern
	
	AnimationFrame Static_Image = {0};
  byte currentAnimation = 0;

	void(*OnComplete)();      // Callback on completion of pattern

	void init();

	// Constructor - calls base-class constructor to initialize strip
	NeoPattern(uint16_t pixels, uint8_t pin, uint8_t type, void(*callback)())
		: Adafruit_NeoPixel(pixels, pin, type)
	{
		AutoReverse = false;
		OnComplete = callback;
	}

	// Update the pattern
	void Update();

	// Increment the Index and reset at the end
	void Increment();

	// Reverse pattern direction
	void Reverse();

//#pragma region Patterns
	void NoneInit();
	void NoneUpdate();

	void IdleInit(uint32_t color1);
	void IdleUpdate();

	void RainbowCycleInit(uint8_t interval, direction dir = FORWARD);
	void RainbowCycleUpdate();

	void TheaterChaseInit(uint32_t color1, uint32_t color2, uint8_t interval, direction dir = FORWARD);
	void TheaterChaseUpdate();

	void ColorWipeInit(uint32_t color, uint8_t interval, direction dir = FORWARD);
	void ColorWipeUpdate();

	void ScannerInit(uint32_t color1, uint8_t interval);
	void ScannerUpdate();

	void FadeInit(uint32_t color1, uint32_t color2, uint16_t steps, uint8_t interval, direction dir = FORWARD);
	void FadeUpdate();

	void EllipsisInit(uint32_t color1, uint32_t color2, uint8_t interval, direction dir = FORWARD);
	void EllipsisUpdate();

	void PulsingInit(uint32_t color1, uint8_t interval, bool autoReverse, direction dir = FORWARD);
	void PulsingUpdate();

	void SolidInit(uint32_t color1);
	void SolidUpdate();

	void WedgeInit(uint32_t color1, uint32_t color2, uint8_t interval, bool autoReverse);
	void WedgeUpdate();

	void SmileInit(uint32_t color1, uint32_t color2, uint32_t color3, uint8_t interval);
	void SmileUpdate();

	void DirectionLeftInit(uint32_t color1, uint32_t color2);
	void DirectionLeftUpdate();

	void DirectionRightInit(uint32_t color1, uint32_t color2);
	void DirectionRightUpdate();

	void DirectionUpInit(uint32_t color1, uint32_t color2);
	void DirectionUpUpdate();

	void DirectionDownInit(uint32_t color1, uint32_t color2);
	void DirectionDownUpdate();

	void ShowImageInit();
	void ShowImageUpdate();

	void ShowAnimationInit();
	void ShowAnimationUpdate();
//#pragma endregion Patterns

//#pragma region Helpers
	// Calculate 50% dimmed version of a color (used by ScannerUpdate)
	uint32_t DimColor(uint32_t color)
	{
		// Shift R, G and B components one bit to the right
		uint32_t dimColor = Color(Red(color) >> 1, Green(color) >> 1, Blue(color) >> 1);
		return dimColor;
	}

	// Set all pixels to a color
	void ColorSet(uint32_t color)
	{
		for (uint16_t i = 0; i < numPixels(); i++)
		{
			setPixelColor(i, color);
		}
	}

	// Returns the Red component of a 32-bit color
	uint8_t Red(uint32_t color)
	{
		return (color >> 16) & 0xFF;
	}

	// Returns the Green component of a 32-bit color
	uint8_t Green(uint32_t color)
	{
		return (color >> 8) & 0xFF;
	}

	// Returns the Blue component of a 32-bit color
	uint8_t Blue(uint32_t color)
	{
		return color & 0xFF;
	}

	// Input a value 0 to 255 to get a color value.
	// The colours are a transition r - g - b - back to r.
	uint32_t Wheel(byte WheelPos)
	{
		WheelPos = 255 - WheelPos;
		if (WheelPos < 85)
		{
			return Color(255 - WheelPos * 3, 0, WheelPos * 3);
		}
		else if (WheelPos < 170)
		{
			WheelPos -= 85;
			return Color(0, WheelPos * 3, 255 - WheelPos * 3);
		}
		else
		{
			WheelPos -= 170;
			return Color(WheelPos * 3, 255 - WheelPos * 3, 0);
		}
	}
//#pragma endregion Helpers
};

