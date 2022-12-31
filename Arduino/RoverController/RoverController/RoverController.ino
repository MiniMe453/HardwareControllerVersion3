#include <Adafruit_Thermal.h>
#include <Uduino.h>
#include <LiquidCrystal.h>
#include <TM1637.h>

#define PRINTER_TX 


Uduino uduino("RoverController");
LiquidCrystal lcd(3, 4, 5, 6, 7, 8);
TM1637 tm1(18, 19);
TM1637 tm2(16, 17);
Adafruit_Thermal printer(&Serial3);

int* digitalInputArray = 0;
int digitalInputArraySize;

int* analogInputArray = 0;
int analogInputArraySize;

int* ledOutputArray = 0;
int ledOutputArraySize;

//Rotary Encoder variables
int rotaryAPin = 12;
int rotaryBPin = 13;
int counter = 0;
bool interruptCalled = false;
unsigned long timeSinceLastInterrupt;
unsigned long timeSinceLastMessage;
unsigned long interruptResetDelay = 25;
unsigned long messageDelay = 10;

int aState;
int aLastState;

int aPinCounter;
int aPinCurrentState = LOW;
int aPinReading;
int bPinCounter;
int bPinCurrentState = LOW;
int bPinReading;
int debounce_count = 2;


void setup() {
  Serial.begin(9600);
  Serial3.begin(19200);
  printer.begin();

  lcd.begin(16, 2);

  uduino.addCommand("led", SetLEDPins);
  uduino.addCommand("lcd", UpdateLCD);
  uduino.addCommand("initD", InitDigital);
  uduino.addCommand("initA", InitAnalog);
  uduino.addCommand("initO", InitOutput);
  uduino.addCommand("writeTM1", WriteTM1Display);
  uduino.addCommand("writeTM2", WriteTM2Display);
  uduino.addCommand("doNth", DoNothing);

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

  //This makes it accurate. DO NOT DELETE
  ReadEncoders();
  SoftwareDebouncer(rotaryAPin, aPinReading, aPinCurrentState, aPinCounter);
  SoftwareDebouncer(rotaryBPin, bPinReading, bPinCurrentState, bPinCounter);

  if ((aPinCurrentState == 0 || bPinCurrentState == 0) && !interruptCalled) {
    interruptCalled = true;
    ReadEncoders();
    timeSinceLastInterrupt = millis();
  }

  //Serial.println(counter);

  String serialLine = "_";

  for (int i = 0; i < digitalInputArraySize; i++) {
    serialLine += String(digitalRead(digitalInputArray[i]));
  }

  for (int i = 0; i < analogInputArraySize; i++) {
    serialLine += " ";
    serialLine += String(analogRead(analogInputArray[i]));
  }

  serialLine += " ";
  serialLine += String(counter);

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

  if (millis() - timeSinceLastMessage > messageDelay) {
    uduino.println(serialLine);

    timeSinceLastMessage = millis();
  }
  //Serial.println(counter);
  //uduino.delay(5);

  //Serial.println(serialLine);

  //uduino.println(counter);
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

void DoNothing()
{
  return;
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

  //Input Array: [Array Size, Pin, PinMode, Pin, PinMode, etc.]
  //Init a new array with the size of our input parameters
  if (ledOutputArray != 0)
    return;

  ledOutputArray = new int[arrLength];
  ledOutputArraySize = arrLength;

  //Move to the first value inside the parameter array
  arg = uduino.next();

  if (arrLength > 0) {
    for (int i = 0; i < arrLength; i++) {
      if (arg != NULL) {
        int pin = uduino.charToInt(arg);

        pinMode(pin, OUTPUT);
        ledOutputArray[i] = pin;
      }

      arg = uduino.next();
    }
  }
}

void ReadEncoders()  //bool readAState, bool secondPin
{
  aState = digitalRead(rotaryAPin);
  // If the previous and the current state of the outputA are different, that means a Pulse has occured
  if (aState != aLastState) {
    // If the outputB state is different to the outputA state, that means the encoder is rotating clockwise
    if (digitalRead(rotaryBPin) != aState) {
      if (counter + 1 <= 512)
        counter++;
    } else {
      if (counter - 1 >= 0)
        counter--;
    }
  }
  aLastState = aState;  // Updates the previous state of the outputA with the current state
}

int SoftwareDebouncer(int pin, int& pinReading, int& pinCurrentState, int& pinCounter) {
  bool inLoop = true;
  int loopCounter = 0;

  while (inLoop && loopCounter < 5) {
    loopCounter++;
    pinReading = digitalRead(pin);
    if (pinReading == pinCurrentState && pinCounter > 0) {
      pinCounter--;
    }

    if (pinReading != pinCurrentState) {
      pinCounter++;
    }

    if (pinCounter >= debounce_count) {
      pinCounter = 0;
      pinCurrentState = pinReading;
      return pinCurrentState;
    }
  }

  return -1;
}