#include <SPI.h>

#include <deprecated.h>
#include <MFRC522.h>
#include <MFRC522Extended.h>
#include <require_cpp11.h>

#define RST_PIN         9
#define SS_PIN          10

MFRC522 mfrc522(SS_PIN, RST_PIN);

//card constants
int bufferSize = 34;
int blocks = 64;
byte blockByteSize = 16;

void setup() {
  Serial.begin(9600);
  SPI.begin();
  mfrc522.PCD_Init();
  Serial.println("Initialized");
}

void loop() {
  // generate the key
  MFRC522::MIFARE_Key key;
  for(byte i = 0; i < 6; i++) key.keyByte[i] = 0xFF;

  if(!mfrc522.PICC_IsNewCardPresent()){
    return;
  }
  if(!mfrc522.PICC_ReadCardSerial()){
    return;
  }

  Serial.println("**Card found**");

  byte buffer[bufferSize];

  MFRC522::StatusCode status = readFromBlocks(&key, buffer);

  if(status != MFRC522::STATUS_OK){
    Serial.print(F("Writing operation failed: "));
    Serial.println(mfrc522.GetStatusCodeName(status));
  }
  else{  
    for(int i = 0; i < bufferSize; i++) Serial.print((char)buffer[i]); 
    Serial.println();
  }
}

MFRC522::StatusCode readFromBlocks(MFRC522::MIFARE_Key *key, byte *buffer){

  MFRC522::StatusCode status;
  int trailerBlock = 3;
  int bufferSector = 0;  
  byte len = 18;

  for(int block = 1; block < blocks; block++){    
    //Serial.println("**Block " + String(block) + "**");    
    if(block == trailerBlock){
      trailerBlock += 4;
      //Serial.println("skipping trailer block...");
      continue;
    }
 
    //////////////////////////////////////
    //        authenticate block        //
    //////////////////////////////////////
    status = mfrc522.PCD_Authenticate(MFRC522::PICC_CMD_MF_AUTH_KEY_A, block, key, &(mfrc522.uid));
    if(!status == MFRC522::STATUS_OK){
      //Serial.print(F("PCD_Authenticate() failed: "));
      //Serial.println(mfrc522.GetStatusCodeName(status));
      break;
    }
    //else Serial.println(F("PCD_Authenticate() success: "));

    //////////////////////////////////////
    //         read from block          //
    //////////////////////////////////////
    status = mfrc522.MIFARE_Read(block, &buffer[bufferSector], &len);
    if (status != MFRC522::STATUS_OK) {
      //Serial.print(F("Reading failed: "));
      //Serial.println(mfrc522.GetStatusCodeName(status));
      break;
    }
    //else Serial.println(F("PCD_Authenticate() success: "));

    bufferSector += 16;
    if(bufferSector > bufferSize) break;
  }

  mfrc522.PICC_HaltA();
  mfrc522.PCD_StopCrypto1();
  return status;
}
