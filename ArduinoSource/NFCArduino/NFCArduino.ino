#include <SPI.h>

#include <deprecated.h>
#include <MFRC522.h>
#include <MFRC522Extended.h>
#include <require_cpp11.h>

#define RST_PIN         9
#define SS_PIN          10

MFRC522 mfrc522(SS_PIN, RST_PIN);

// card constants
int blocks = 64;
byte blockByteSize = 16;

// max buffer size for mifare classic 1k tags
int bufferSize = 752;

// constants
String establishedConnectionKey = "setup";
String waitForConnectionKey = "setConnection";

float timeout = 1000;
float baudrate = 9600;

bool isConnected;

void setup(){
  Serial.begin(baudrate);
  Serial.setTimeout(timeout);

  SPI.begin();
  mfrc522.PCD_Init();
  
  isConnected = false;
  WaitUntilConnection();
}

void loop(){
  // generate the key
  MFRC522::MIFARE_Key key;
  for(byte i = 0; i < 6; i++) key.keyByte[i] = 0xFF;

  if(!mfrc522.PICC_IsNewCardPresent()){
    return;
  }
  if(!mfrc522.PICC_ReadCardSerial()){
    return;
  }

  byte buffer[bufferSize];
  MFRC522::StatusCode status = readFromBlocks(&key, buffer);

  if(status != MFRC522::STATUS_OK){
    Serial.println(mfrc522.GetStatusCodeName(status));
    return;
  } 

  String data((char*)buffer);
  //data.trim();
  Serial.println(data); 
}

// Callback function for resending spoofed data
void serialEvent(){
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

MFRC522::StatusCode readFromBlocks(MFRC522::MIFARE_Key *key, byte *buffer){
  MFRC522::StatusCode status;
  int trailerBlock = 3;
  int bufferSector = 0;  
  byte len = 18;

  for(int block = 1; block < blocks; block++){       
    if(block == trailerBlock){
      trailerBlock += 4;
      continue;
    }
 
    //////////////////////////////////////
    //        authenticate block        //
    //////////////////////////////////////
    status = mfrc522.PCD_Authenticate(MFRC522::PICC_CMD_MF_AUTH_KEY_A, block, key, &(mfrc522.uid));
    if(!status == MFRC522::STATUS_OK){
      break;
    }

    //////////////////////////////////////
    //         read from block          //
    //////////////////////////////////////
    status = mfrc522.MIFARE_Read(block, &buffer[bufferSector], &len);
    if (status != MFRC522::STATUS_OK) {
      break;
    }

    for(int i = bufferSector; i < (bufferSector + 16); i++){
      char test = (char)buffer[i];
      if(test == '\0'){
        break;
      }
    }
    
    bufferSector += 16;
    if(bufferSector > bufferSize) break;
  }

  mfrc522.PICC_HaltA();
  mfrc522.PCD_StopCrypto1();
  return status;
}
