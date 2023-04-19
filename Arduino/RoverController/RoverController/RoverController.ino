#include <Adafruit_Thermal.h>
#include <Uduino.h>
#include <LiquidCrystal.h>
#include <TM1637.h>
#include <Encoder.h>
#include "printerLutTable.h"

#define INPUT_FRAMERATE 30.0

int SPDT_3WAY_SWITCH_PIN = A10;


Uduino uduino("RoverController");
LiquidCrystal lcd(3, 4, 5, 6, 7, 8);
TM1637 tm1(18, 19);
TM1637 tm2(16, 17);
Adafruit_Thermal printer(&Serial3);
Encoder rotaryEnc(21, 20);

int* dIoArr = 0;
int* dIoRdArr = 0;
uint8_t dIoArrSize;

int* aIoArr = 0;
uint8_t aIoArrSize;

int* ledArr = 0;
uint8_t ledArrSize;

//Rotary Encoder variables
int rtrA = 12;
int rtrB = 13;
int counter = 0;
long oldPos  = -999;

unsigned long timeSinceLastMessage;
unsigned long interruptResetDelay = 25;
unsigned long messageDelay = 10;


//Object Scan Variables
String date = "May 12, 1985";
String dateTime = "";
int seconds;
int minutes;
int hours;
unsigned long timeSinceLastDateInterrupt;

bool isPrinting = false;

void setup() {
  Serial.begin(9600);
  Serial3.begin(19200);
  printer.begin();
  //printer.setHeatConfig(8, 10, 10);

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

  if(isPrinting)
    return;

  long newPosition = rotaryEnc.read();

  if (newPosition > oldPos) 
  {
    counter++;
  }
  else if(newPosition < oldPos)
  {
    counter--;
  }
  
  oldPos = newPosition;


  if(counter > 1024)
    counter = 1024;
  else if (counter < 0)
    counter = 0;

  String serialLine = "_";

  for (int i = 0; i < dIoArrSize; i++) {
    bool invertInput = dIoArr[i] == 11;

    int prevReadValue = dIoRdArr[i];
    bool skipValueSet = prevReadValue == 1;

    if(!skipValueSet)
    {
      if(invertInput)
        dIoRdArr[i] = !digitalRead(dIoArr[i]);
      else
        dIoRdArr[i] = digitalRead(dIoArr[i]);
    }

    serialLine += String(dIoRdArr[i]);
  }

  for (int i = 0; i < aIoArrSize; i++) {
    serialLine += " ";
    serialLine += String(analogRead(aIoArr[i]));
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

  if (millis() - timeSinceLastDateInterrupt > 1000) {
    timeSinceLastDateInterrupt = millis();

    SetDateTimeString();
  }

    if (millis() - timeSinceLastMessage > (1.0/INPUT_FRAMERATE) * 1000.0) {
      uduino.println(serialLine);

      timeSinceLastMessage = millis();
      ResetdIoRdArr();
    }
}

void ResetdIoRdArr()
{
  for (int i = 0; i < dIoArrSize; i++) {
    bool invertInput = dIoArr[i] == 11;

    if(invertInput)
      dIoRdArr[i] = !digitalRead(dIoArr[i]);
    else
      dIoRdArr[i] = digitalRead(dIoArr[i]);
  
  }
}

void SetDateTimeString()
{
  seconds++;

  if(seconds == 60)
  {
    seconds = 0;
    minutes++;
    if(minutes == 60)
     {
       minutes=0;
       hours++;
     } 
  }

  String min = "";
  String sec = "";
  String hrs = "";

  if(minutes < 10)
    min = "0" + String(minutes);
  else
    min = String(minutes);
  
  if(seconds < 10)
    sec = "0" + String(seconds);
  else
    sec = String(seconds);

  if(hours < 10)
    hrs = "0" + String(hours);
  else
    hrs = String(hours);

  dateTime = date + " " + hrs +":"+min+":"+sec;
}

void SetLEDPins() {
  char* arg;
  arg = uduino.next();

  for (int i = 0; i < ledArrSize; i++) {
    if (arg != NULL)
    {
      digitalWrite(ledArr[i], uduino.charToInt(arg));
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
  if (dIoArr != 0)
    return;

  dIoArr = new int[arrLength];
  dIoRdArr = new int[arrLength];
  dIoArrSize = arrLength;

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
        dIoArr[i] = pin;
        dIoRdArr[i] = 0;
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
  if (aIoArr != 0)
    return;

  aIoArr = new int[arrLength];
  aIoArrSize = arrLength;

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
        aIoArr[i] = pin;
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
    ledArr = new int[arrLength];
    ledArrSize = arrLength;
  }

  //Move to the first value inside the parameter array
  arg = uduino.next();

  if (ledArrSize > 0) {
    for (int i = startingIndex; i < startingIndex + numOfPinsInMessage; i++) {
      if (arg != NULL) {
        int pin = uduino.charToInt(arg);

        pinMode(pin, OUTPUT);
        ledArr[i] = pin;
      }

      arg = uduino.next();
    }
  }

  seconds = 15;
  minutes = 34;
  hours = 8;
}

void PrintObjectScan()
{
  char* arg;
  arg = uduino.next();

  /**
  argument setup
  0 - numOfSurfaceProperties
  1 - ? - surfaceProperties
  **/

  int stringLUTindex = uduino.charToInt(arg);

  // if(stringLUTindex > 13)
  // {
  //   uduino.println("prt_finished");
  //   return;
  // }


  //We are now ready for surface properties, but we loop this inside the printing process.

  isPrinting = true;
  printer.justify('C');
  printer.doubleHeightOn();
  printer.underlineOn();
  printer.println("OBJ_SCAN");
  printer.underlineOff();
  printer.doubleHeightOff();

  printer.justify('L');
  printer.println(" ");
  PrintBoldLine("SCAN_TIME:");
  printer.println(dateTime);
  PrintBoldLine("OBJ_TYPE:");
  printer.println(printer_lines[stringLUTindex][0]);
  PrintBoldLine("OBJ_DIST:");
  printer.println("0.37m");
  printer.doubleHeightOn();
  PrintBoldLine("DATA LOGS");
  printer.doubleHeightOff();
  printer.println(" ");

  if(printer_lines[stringLUTindex][1] == "No data logs")
  {
    printer.println("No data logs.");
  }
  else
  {
    printer.println(printer_lines[stringLUTindex][1]);
    printer.println(printer_lines[stringLUTindex][2]);
    printer.println(printer_lines[stringLUTindex][3]);
    printer.println(printer_lines[stringLUTindex][4]);
    printer.println(printer_lines[stringLUTindex][5]);
    printer.println(printer_lines[stringLUTindex][6]);
  }

  printer.println(" ");
  printer.doubleHeightOn();
  PrintBoldLine("SURFACE READINGS");
  printer.doubleHeightOff();
  printer.println(" ");
  PrintBoldLine("TEMPERATURE:");
  printer.println(printer_lines[stringLUTindex][9]);
  PrintBoldLine("RADIATION:");
  printer.println(printer_lines[stringLUTindex][7]);
  PrintBoldLine("MAGNETIC FIELD:");
  printer.println(printer_lines[stringLUTindex][8]);
  printer.println(" ");

  printer.println("Scan Complete");
  printer.println("Hardware Version: 3.40.1f");
  printer.println("Software version: 1.05a");
  printer.println("All data contained on this paper");
  printer.println("is property of Black Isle Space.");
  printer.println("Unauthorized distribution will");
  printer.println("be subject to prosecution.");
  printer.println(" ");
  printer.println(" ");
  uduino.println("prt_finished");
  isPrinting = false;
}

void PrintBoldLine(String line)
{
  printer.boldOn();
  printer.println(line);
  printer.boldOff();
}

void SimplePrint()
{
  char* arg;
  arg = uduino.next();

  printer.justify('L');
  printer.println(arg);
  printer.println(" ");
}