#include <SPI.h>
#include <SerialCommands.h>



// constants
String establishedConnectionKey = "setup";
String waitForConnectionKey = "setConnection";

float timeout = 1000;
float baudrate = 9600;

bool isConnected;

void setup() {
  Serial.begin(baudrate);
  Serial.setTimeout(timeout);
  
  isConnected = false;
  WaitUntilConnection();
}

void loop() {
}

void serialEvent(){
  //currently only implementation, for when data is spoofed
  if(Serial.available() > 0){
    String data = Serial.readStringUntil('\n');
    data.trim();
    Serial.println(data);
  }
}

void WaitUntilConnection(){
  Serial.println(waitForConnectionKey);
  while(!isConnected){
    if(Serial.available() > 0){
      String data = Serial.readStringUntil('\n');
      data.trim();
      if(data == establishedConnectionKey){
        isConnected = true;
      }
    }
  }
}
