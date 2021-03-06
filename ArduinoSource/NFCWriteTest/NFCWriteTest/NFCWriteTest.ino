#include <SPI.h>

#include <deprecated.h>
#include <MFRC522.h>
#include <MFRC522Extended.h>
#include <require_cpp11.h>

#define RST_PIN         9
#define SS_PIN          10

MFRC522 mfrc522(SS_PIN, RST_PIN);

String debugtxt = "";
//String debugtxt = "{\"type\":\"Ability\",\"name\":\"Sub1fromEach\",\"desc\":\"Subtracts one of each vars of multiple foes, and buffs each of the players vars\",\"ptCst\":1,\"fx\":[{\"trgts\":\"enemy\",\"trgtType\":\"multiple\",\"varchng\":[{\"var\":\"health\",\"type\":\"additive\",\"chng\":-1,\"offst\":0,\"trns\":1},{\"var\":\"ability\",\"type\":\"additive\",\"chng\":-1,\"offst\":0,\"trns\":1},{\"var\":\"victory\",\"type\":\"additive\",\"chng\":-1,\"offst\":0,\"trns\":1},{\"var\":\"damage\",\"type\":\"additive\",\"chng\":-1,\"offst\":0,\"trns\":1}]},{\"trgts\":\"user\",\"variableChanges\":[{\"var\":\"health\",\"type\":\"additive\",\"chng\":1,\"offst\":0,\"trns\":1},{\"var\":\"ability\",\"type\":\"additive\",\"chng\":1,\"offst\":0,\"trns\":1},{\"var\":\"victory\",\"type\":\"additive\",\"chng\":1,\"offst\":0,\"trns\":1},{\"var\":\"damage\",\"type\":\"additive\",\"chng\":1,\"offst\":0,\"trns\":1}]}]}";
//String debugtxt = "{\"typeOf\":\"Character\",\"name\":\"Krodha\",\"description\":\"A strong bezerker who deals alot of damage in exchange for movement.\",\"maxHealth\":15,\"maxAbilityPoints\":5}";
//String debugtxt = "{\"typeOf\":\"Ability\",\"name\":\"Mending salve\",\"description\":\"Heal yourself for 2 HP.\",\"damage\":0,\"canDamageMultiple\":false,\"heals\":2,\"pointCost\":3}";

//card constants
int blocks = 64;
byte blockByteSize = 16;

//max buffer size for mifare classic 1k tags
int bufferSize = 752;

void serialEvent(){
  if(Serial.available() > 0){
    debugtxt = Serial.readStringUntil('\n');
    Serial.println(debugtxt);
  }
}

void setup() {
  Serial.begin(9600);
  SPI.begin();
  mfrc522.PCD_Init();
  Serial.println("Initialized");
}

void loop() {
  if(debugtxt.length() > 752){
    Serial.println("error: string too big to fit on tag");
    return;
  }
  
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

  // add a null terminator to end of string
  buffer[debugtxt.length()] = '\0';
  
  // fill rest of buffer with whitespace
  for(int i = (debugtxt.length() + 1); i < bufferSize; i++) buffer[i] = ' ';

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
