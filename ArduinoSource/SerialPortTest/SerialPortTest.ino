// constants
String establishedConnectionKey = "setup";
String waitForConnectionKey = "setConnection";

bool isConnected;

void setup() {
  Serial.begin(9600);
  isConnected = false;
  WaitUntilConnection();
}

void loop() {
  if(isConnected){
    Serial.println("IM active!!!!");
    
    /*if(Serial.available() > 0){
      String data = Serial.readString();
      data.trim();
      Serial.println(data);
    }*/
  }
  delay(2000);
}

void WaitUntilConnection(){
  Serial.println(waitForConnectionKey);
  while(!isConnected){
    if(Serial.available() > 0){
      String data = Serial.readString();
      data.trim();
      if(data == establishedConnectionKey){
        isConnected = true;
      }
    }
  }
}
