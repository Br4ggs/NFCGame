#include <SPI.h>

#include <deprecated.h>
#include <MFRC522.h>
#include <MFRC522Extended.h>
#include <require_cpp11.h>

#define RST_PIN         9
#define SS_PIN          10

MFRC522 mfrc522(SS_PIN, RST_PIN);

String debugtxt = "hallo dit is Benno van den Brink";
//String debugtxt = "{\"typeOf\":\"Character\",\"name\":\"TestName\",\"description\":\"TestDescription\",\"maxHealth\":15,\"maxAbilityPoints\":10}";
String delimiter = "#";

//card constants
int blocks = 64;
byte blockByteSize = 16;

//max buffer size for mifare classic 1k tags
int bufferSize = 752;

void setup() {
  Serial.begin(9600);
  SPI.begin();
  mfrc522.PCD_Init();
  Serial.println("Initialized");
}

void loop() {
  // generate the key
  MFRC522::MIFARE_Key key;
  for(byte i = 0; i < 6; i++){
    key.keyByte[i] = 0xFF;
  }

  if(!mfrc522.PICC_IsNewCardPresent()){
    return;
  }

  if(!mfrc522.PICC_ReadCardSerial()){
    return;
  }

  Serial.println("**Card found**");
  
  byte buffer[bufferSize];

  // write debug text into buffer
  debugtxt.getBytes(buffer, bufferSize);
  // fill rest of buffer with whitespace
  for(int i = debugtxt.length(); i < bufferSize; i++) buffer[i] = ' ';

  Serial.println();
  for(int i = 0; i < bufferSize; i++) Serial.print((char)buffer[i]); 

  MFRC522::StatusCode status = writeToBlocks(&key, buffer, bufferSize);
}

MFRC522::StatusCode writeToBlocks(MFRC522::MIFARE_Key *key, byte *buffer, int len){ 
  /*for(int i = 0; i < len; i++){
      Serial.print((char)buffer[i]);
  }
  Serial.println();*/
  
  // start at block 1 since block 0 is read-only manufacturer data
  // every 4th block of a sector we skip, since this is a trailer block containing metadata
  int trailerBlock = 3;
  int bufferSector = 0;
  
  for(int block = 1; block < blocks; block++){
    
    Serial.println("**Block " + String(block) + "**");    
    if(block == trailerBlock){
      trailerBlock += 4;
      Serial.println("skipping trailer block...");
      continue;
    }

    //////////////////////////////////////
    //        authenticate block        //
    //////////////////////////////////////
    MFRC522::StatusCode status = mfrc522.PCD_Authenticate(MFRC522::PICC_CMD_MF_AUTH_KEY_A, block, key, &(mfrc522.uid));
    if(!status == MFRC522::STATUS_OK){
      Serial.print(F("PCD_Authenticate() failed: "));
      Serial.println(mfrc522.GetStatusCodeName(status));
      break;
    }
    else Serial.println(F("PCD_Authenticate() success: "));

    //////////////////////////////////////
    //          write to block          //
    //////////////////////////////////////
    status = mfrc522.MIFARE_Write(block, &buffer[bufferSector], blockByteSize);
    if (status != MFRC522::STATUS_OK) {
      Serial.print(F("MIFARE_Write() failed: "));
      Serial.println(mfrc522.GetStatusCodeName(status));
      break;
    }
    else Serial.println(F("MIFARE_Write() success: "));

    bufferSector += 16;

    //check if complete buffer has been written yet, if so break from loop
    if(bufferSector > bufferSize) break;
  }


  // very important you call these or board won't be able to read tags a second time
  mfrc522.PICC_HaltA();
  mfrc522.PCD_StopCrypto1();
}
