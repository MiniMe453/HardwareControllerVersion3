#include <Adafruit_Thermal.h>
#include <Uduino.h>
#include <LiquidCrystal.h>
#include <TM1637.h>
#include <Encoder.h>

#define INPUT_FRAMERATE 30.0

int SPDT_3WAY_SWITCH_PIN = A10;


Uduino uduino("RoverController");
LiquidCrystal lcd(3, 4, 5, 6, 7, 8);
TM1637 tm1(18, 19);
TM1637 tm2(16, 17);
Adafruit_Thermal printer(&Serial3);
Encoder rotaryEnc(20, 21);

int* digitalInputArray = 0;
int* digitalReadArray = 0;
int digitalInputArraySize;

int* analogInputArray = 0;
int analogInputArraySize;

int* ledOutputArray = 0;
int ledOutputArraySize;

//Rotary Encoder variables
int rotaryAPin = 12;
int rotaryBPin = 13;
int counter = 0;
long oldPosition  = -999;

bool interruptCalled = false;
unsigned long timeSinceLastInterrupt;
unsigned long timeSinceLastMessage;
unsigned long interruptResetDelay = 25;
unsigned long messageDelay = 10;

const char* materialTypes[] = {"Strontium", "Tungsten", "Iron", "Aluminum", "Lead", "Carbon", "Radium", "Cobalt", "Sulfur", "Copper", "Titanium", "Potassium", "Sodium", "Unknown"};

void setup() {
  Serial.begin(9600);
  Serial3.begin(19200);
  printer.begin();

  pinMode(A3, INPUT);

  lcd.begin(16, 2);

  uduino.addCommand("led", SetLEDPins);
  uduino.addCommand("lcd", UpdateLCD);
  uduino.addCommand("initD", InitDigital);
  uduino.addCommand("initA", InitAnalog);
  uduino.addCommand("initO", InitOutput);
  uduino.addCommand("writeTM1", WriteTM1Display);
  uduino.addCommand("writeTM2", WriteTM2Display);
  uduino.addCommand("pobs", PrintObjectScan);
  uduino.addCommand("prt", SimplePrint);

  lcd.setCursor(0, 0);
  lcd.print("Hello world!");
  lcd.setCursor(0, 1);
  lcd.print(10);

  tm1.init();
  tm1.set(1);
  tm2.init();
  tm2.set(1);
}

void loop() {
  uduino.update();

  long newPosition = rotaryEnc.read();

  if (newPosition != oldPosition) 
  {
    oldPosition = newPosition;
    counter = newPosition;
  }

  String serialLine = "_";

  for (int i = 0; i < digitalInputArraySize; i++) {
    int prevReadValue = digitalReadArray[i];
    bool skipValueSet = prevReadValue == 1;

    if(!skipValueSet)
    {
      digitalReadArray[i] = digitalRead(digitalInputArray[i]);
    }

    serialLine += String(digitalReadArray[i]);
  }

  for (int i = 0; i < analogInputArraySize; i++) {
    serialLine += " ";
    serialLine += String(analogRead(analogInputArray[i]));
  }

  serialLine += " ";
  serialLine += String(counter);

  serialLine += " ";
  serialLine += String(analogRead(A3));

  //Serial data structure
  /**
0 - testpin1
1 - testPin2
2 - testPin3
3 - testPin4
4 - take photo button
5 - encoder buttons

**/

  if (millis() - timeSinceLastInterrupt > interruptResetDelay && interruptCalled) {
    interruptCalled = false;
  }

  if (millis() - timeSinceLastMessage > (1.0/INPUT_FRAMERATE) * 1000.0) {
    uduino.println(serialLine);

    timeSinceLastMessage = millis();

    ResetDigitalReadArray();
  }
}

void ResetDigitalReadArray()
{
  for (int i = 0; i < digitalInputArraySize; i++) {
    
    if(digitalRead(digitalInputArray[i]) == 1)
      continue;
    
    digitalReadArray[i] = 0;
  }
}

void SetLEDPins() {
  char* arg;
  arg = uduino.next();

  for (int i = 0; i < ledOutputArraySize; i++) {
    if (arg != NULL)
    {
      digitalWrite(ledOutputArray[i], uduino.charToInt(arg));
    }

    arg = uduino.next();
  }
}

void UpdateLCD() {

  char* arg;
  arg = uduino.next();

  if (arg != NULL) {
    lcd.setCursor(0, 0);
    lcd.print(arg);
    arg = uduino.next();
    lcd.setCursor(0, 1);
    lcd.print(arg);
  }

  // lcd.clear();
  // lcd.setCursor(0, 0);
  // lcd.print("Hello world!");
  // lcd.setCursor(0, 1);
  // lcd.print(10);
}

void WriteTM1Display()
{
  char* arg;
  arg = uduino.next();

  for(int i = 0; i < 4; i++)
  {
    tm1.display(i, uduino.charToInt(arg));
    arg = uduino.next();
  }

  tm1.point(uduino.charToInt(arg));
}

void WriteTM2Display()
{
  char* arg;
  arg = uduino.next();

  for(int i = 0; i < 4; i++)
  {
    tm2.display(i, uduino.charToInt(arg));
    arg = uduino.next();
  }

  tm2.point(uduino.charToInt(arg));
}

void InitDigital() 
{
  char* arg;
  arg = uduino.next();
  int arrLength = uduino.charToInt(arg);

  //Input Array: [Array Size, Pin, PinMode, Pin, PinMode, etc.]
  //Init a new array with the size of our input parameters
  if (digitalInputArray != 0)
    return;

  digitalInputArray = new int[arrLength];
  digitalReadArray = new int[arrLength];
  digitalInputArraySize = arrLength;

  //Move to the first value inside the parameter array
  arg = uduino.next();

  if (arrLength > 0) {
    for (int i = 0; i < arrLength; i++) {
      if (arg != NULL) {
        int pin = uduino.charToInt(arg);

        //This is when i was sending the pinMode as well. 
        //I removed this because it meant that the message was too long
        // arg = uduino.next();
        // int pinM = uduino.charToInt(arg);

        pinMode(pin, INPUT_PULLUP);
        digitalInputArray[i] = pin;
        digitalReadArray[i] = 0;
      }

      arg = uduino.next();
    }
  }
}

void InitAnalog() {
  char* arg;
  arg = uduino.next();
  int arrLength = uduino.charToInt(arg);

  //Input Array: [Array Size, Pin, PinMode, Pin, PinMode, etc.]
  //Init a new array with the size of our input parameters
  if (analogInputArray != 0)
    return;

  analogInputArray = new int[arrLength];
  analogInputArraySize = arrLength;

  //Move to the first value inside the parameter array
  arg = uduino.next();

  if (arrLength > 0) {
    for (int i = 0; i < arrLength; i++) {
      if (arg != NULL) {
        int pin = uduino.charToInt(arg);

        //This is when i was sending the pinMode as well. 
        //I removed this because it meant that the message was too long
        // arg = uduino.next();
        // int pinM = uduino.charToInt(arg);

        pinMode(pin, INPUT);
        analogInputArray[i] = pin;
      }

      arg = uduino.next();
    }
  }
}

void InitOutput() {
  char* arg;
  arg = uduino.next();
  int arrLength = uduino.charToInt(arg);
  arg = uduino.next();
  int startingIndex= uduino.charToInt(arg);
  arg = uduino.next();
  int numOfPinsInMessage = uduino.charToInt(arg);

  //Input Array: [Array Size, Pin, PinMode, Pin, PinMode, etc.]
  //Init a new array with the size of our input parameters
  if (arrLength != -1)
  {
    ledOutputArray = new int[arrLength];
    ledOutputArraySize = arrLength;
  }

  //Move to the first value inside the parameter array
  arg = uduino.next();

  if (ledOutputArraySize > 0) {
    for (int i = startingIndex; i < startingIndex + numOfPinsInMessage; i++) {
      if (arg != NULL) {
        int pin = uduino.charToInt(arg);

        pinMode(pin, OUTPUT);
        ledOutputArray[i] = pin;
      }

      arg = uduino.next();
    }
  }
}

void PrintObjectScan()
{
  char* arg;
  arg = uduino.next();

  /**
  argument setup
  0 - numOfSurfaceProperties
  1 - objName
  2 - objSurfaceDepth
  3 - temperature
  4 - magnetic
  5 - radiation
  6 - dateTime
  7 - surfaceProperties
  **/

  int numOfSurfaceProperties = uduino.charToInt(arg);

  printer.justify('C');
  printer.doubleHeightOn();
  printer.println("SCAN RESULTS");
  printer.doubleHeightOff();

  printer.println("This is a test");

  printer.println(" ");

  printer.println("Hello World!");

  printer.println(" ");

  printer.println("Surface Properties thing");

  printer.println(" ");
  printer.println(" ");
  printer.println(" ");

  uduino.println("prt_finished");
}

void SimplePrint()
{
  char* arg;
  arg = uduino.next();

  printer.justify('L');
  printer.println(arg);
  printer.println(" ");
}