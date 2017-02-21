#include "NeoPattern.h"

void NeoPattern::Update()
{
	if ((millis() - lastUpdate) > Interval) // time to update
	{
		lastUpdate = millis();
		switch (ActivePattern)
		{
		case NONE:
			NoneUpdate();
			break;
		case RAINBOW_CYCLE:
			RainbowCycleUpdate();
			break;
		case THEATER_CHASE:
			TheaterChaseUpdate();
			break;
		case COLOR_WIPE:
			ColorWipeUpdate();
			break;
		case SCANNER:
			ScannerUpdate();
			break;
		case FADE:
			FadeUpdate();
			break;
		case ELLIPSIS:
			EllipsisUpdate();
			break;
		case PULSING:
			PulsingUpdate();
			break;
		case SOLID:
			SolidUpdate();
			break;
		case WEDGE:
			WedgeUpdate();
			break;
		case SMILE:
			SmileUpdate();
			break;
		case IDLE:
			IdleUpdate();
			break;
		case DIRECTION_LEFT:
			DirectionLeftUpdate();
			break;
		case DIRECTION_RIGHT:
			DirectionRightUpdate();
			break;
		case DIRECTION_UP:
			DirectionUpUpdate();
			break;
		case DIRECTION_DOWN:
			DirectionDownUpdate();
			break;
		default:
			break;
		}
	}
}

void NeoPattern::Increment()
{
	if (Direction == FORWARD)
	{
		Index++;
		if (Index >= TotalSteps)
		{
			if (AutoReverse)
			{
				Reverse();
			}
			else
			{
				Index = 0;
				if (OnComplete != NULL)
				{
					OnComplete(); // call the comlpetion callback
				}
			}
		}
	}
	else // Direction == REVERSE
	{
		--Index;
		if (Index <= 0)
		{
			if (AutoReverse)
			{
				Reverse();
			}
			else
			{
				Index = TotalSteps - 1;
				if (OnComplete != NULL)
				{
					OnComplete(); // call the comlpetion callback
				}
			}
		}
	}
}

void NeoPattern::Reverse()
{
	if (Direction == FORWARD)
	{
		Direction = REVERSE;
		Index = TotalSteps - 1;
	}
	else
	{
		Direction = FORWARD;
		Index = 0;
	}
}

#pragma region Patterns
void NeoPattern::NoneInit()
{
	ActivePattern = NONE;
	Interval = 100;
	TotalSteps = 10;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::NoneUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, 0);
	}
	show();
	Increment();
}

void NeoPattern::IdleInit(uint32_t color1)
{
	ActivePattern = IDLE;
	Interval = 100;
	TotalSteps = 10;
	Color1 = color1;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::IdleUpdate()
{
	setPixelColor(0, Color1);
	for (int i = 1; i < numPixels(); i++)
	{
		setPixelColor(i, 0);
	}
	show();
	Increment();
}

void NeoPattern::RainbowCycleInit(uint8_t interval, direction dir)
{
	ActivePattern = RAINBOW_CYCLE;
	Interval = interval;
	TotalSteps = 255;
	Index = 0;
	Direction = dir;
	AutoReverse = false;
}

void NeoPattern::RainbowCycleUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Wheel(((i * 256 / numPixels()) + Index) & 255));
	}
	show();
	Increment();
}

// Initialize for a Theater Chase
void NeoPattern::TheaterChaseInit(uint32_t color1, uint32_t color2, uint8_t interval, direction dir)
{
	ActivePattern = THEATER_CHASE;
	Interval = interval;
	TotalSteps = numPixels();
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = dir;
	AutoReverse = false;
}

// Update the Theater Chase Pattern
void NeoPattern::TheaterChaseUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		if ((i + Index) % 2 == 0)
		{
			setPixelColor(i, Color1);
		}
		else
		{
			setPixelColor(i, Color2);
		}
	}
	show();
	Increment();
}

// Initialize for a ColorWipe
void NeoPattern::ColorWipeInit(uint32_t color, uint8_t interval, direction dir)
{
	ActivePattern = COLOR_WIPE;
	Interval = interval;
	TotalSteps = numPixels();
	Color1 = color;
	Index = 0;
	Direction = dir;
	AutoReverse = false;
}

// Update the Color Wipe Pattern
void NeoPattern::ColorWipeUpdate()
{
	setPixelColor(Index, Color1);
	show();
	Increment();
}

// Initialize for a SCANNNER
void NeoPattern::ScannerInit(uint32_t color1, uint8_t interval)
{
	ActivePattern = SCANNER;
	Interval = interval;
	TotalSteps = (numPixels() - 1) * 2;
	Color1 = color1;
	Index = 0;
	AutoReverse = false;
}

// Update the Scanner Pattern
void NeoPattern::ScannerUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		if (i == Index)  // Scan Pixel to the right
		{
			setPixelColor(i, Color1);
		}
		else if (i == TotalSteps - Index) // Scan Pixel to the left
		{
			setPixelColor(i, Color1);
		}
		else // Fading tail
		{
			setPixelColor(i, DimColor(getPixelColor(i)));
		}
	}
	show();
	Increment();
}

void NeoPattern::FadeInit(uint32_t color1, uint32_t color2, uint16_t steps, uint8_t interval, direction dir)
{
	ActivePattern = FADE;
	Interval = interval;
	TotalSteps = steps;
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = dir;
	AutoReverse = false;
}

void NeoPattern::FadeUpdate()
{
	// Calculate linear interpolation between Color1 and Color2
	// Optimise order of operations to minimize truncation error
	uint8_t red = ((Red(Color1) * (TotalSteps - Index)) + (Red(Color2) * Index)) / TotalSteps;
	uint8_t green = ((Green(Color1) * (TotalSteps - Index)) + (Green(Color2) * Index)) / TotalSteps;
	uint8_t blue = ((Blue(Color1) * (TotalSteps - Index)) + (Blue(Color2) * Index)) / TotalSteps;

	ColorSet(Color(red, green, blue));
	show();
	Increment();
}

void NeoPattern::EllipsisInit(uint32_t color1, uint32_t color2, uint8_t interval, direction dir)
{
	ActivePattern = ELLIPSIS;
	Interval = interval;
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = dir;
	TotalSteps = numPixels();
	AutoReverse = false;
}

void NeoPattern::EllipsisUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		if ((i == Index       % numPixels()) ||
			(i == (Index + 1) % numPixels()) ||
			(i == (Index + 2) % numPixels()))
		{
			setPixelColor(i, Color1);
		}
		else
		{
			setPixelColor(i, Color2);
		}
	}
	show();
	Increment();
}

void NeoPattern::PulsingInit(uint32_t color1, uint8_t interval, bool autoReverse, direction dir)
{
	ActivePattern = PULSING;
	Interval = interval;
	Color1 = color1;
	Index = 0;
	Direction = dir;
	TotalSteps = 5;
	AutoReverse = autoReverse;
}

void NeoPattern::PulsingUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color1);
	}

	// Brightness Increment == 20
	setBrightness(20 * (Index + 1));

	show();
	Increment();
}

void NeoPattern::SolidInit(uint32_t color1)
{
	ActivePattern = SOLID;
	Interval = 100;
	TotalSteps = 10;
	Color1 = color1;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::SolidUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color1);
	}
	show();
	Increment();
}

void NeoPattern::WedgeInit(uint32_t color1, uint32_t color2, uint8_t interval, bool autoReverse)
{
	ActivePattern = WEDGE;
	Interval = interval;
	TotalSteps = 10;
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = autoReverse;
}

void NeoPattern::WedgeUpdate()
{
	// Set background color
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color2);
	}

	if (Index > 6)
	{
		setPixelColor(2, Color1);
		setPixelColor(numPixels() - 2, Color1);
	}

	if (Index > 4)
	{
		setPixelColor(1, Color1);
		setPixelColor(numPixels() - 1, Color1);
	}

	if (Index > 2)
	{
		setPixelColor(0, Color1);
	}

	show();
	Increment();
}

void NeoPattern::SmileInit(uint32_t color1, uint32_t color2, uint32_t color3, uint8_t interval)
{
	ActivePattern = SMILE;
	Interval = interval;
	TotalSteps = 10;
	Color1 = color1;
	Color2 = color2;
	Color3 = color3;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::SmileUpdate()
{
	// Set background color
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color3);
	}

	// Eyes
	setPixelColor(2, Color1);
	setPixelColor(numPixels() - 2, Color1);
	//	setPixelColor(3, Color1);
	//	setPixelColor(numPixels() - 3, Color1);

	// Mouth
	setPixelColor(6, Color2);
	setPixelColor(numPixels() - 6, Color2);
	setPixelColor(7, Color2);
	setPixelColor(numPixels() - 7, Color2);
	setPixelColor(8, Color2);
	setPixelColor(numPixels() - 8, Color2);

	show();
	Increment();
}

void NeoPattern::DirectionLeftInit(uint32_t color1, uint32_t color2)
{
	ActivePattern = DIRECTION_LEFT;
	Interval = 100;
	TotalSteps = 10;
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::DirectionLeftUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color2);
	}

  setPixelColor(3, Color1);
  setPixelColor(4, Color1);
  setPixelColor(5, Color1);

	show();
	Increment();
}

void NeoPattern::DirectionRightInit(uint32_t color1, uint32_t color2)
{
	ActivePattern = DIRECTION_RIGHT;
	Interval = 100;
	TotalSteps = 10;
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::DirectionRightUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color2);
	}

  setPixelColor(13, Color1);
  setPixelColor(12, Color1);
  setPixelColor(11, Color1);

	show();
	Increment();
}

void NeoPattern::DirectionUpInit(uint32_t color1, uint32_t color2)
{
	ActivePattern = DIRECTION_UP;
	Interval = 100;
	TotalSteps = 10;
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::DirectionUpUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color2);
	}

	setPixelColor(1, Color1);
	setPixelColor(0, Color1);
	setPixelColor(15, Color1);

	show();
	Increment();
}

void NeoPattern::DirectionDownInit(uint32_t color1, uint32_t color2)
{
	ActivePattern = DIRECTION_DOWN;
	Interval = 100;
	TotalSteps = 10;
	Color1 = color1;
	Color2 = color2;
	Index = 0;
	Direction = FORWARD;
	AutoReverse = false;
}

void NeoPattern::DirectionDownUpdate()
{
	for (int i = 0; i < numPixels(); i++)
	{
		setPixelColor(i, Color2);
	}

	setPixelColor(7, Color1);
	setPixelColor(8, Color1);
	setPixelColor(9, Color1);

	show();
	Increment();
}
#pragma endregion Patterns
