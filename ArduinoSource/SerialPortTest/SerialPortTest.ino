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
    if(Serial.available() > 0){
      String data = Serial.readString();
      data.trim();
      Serial.println(data);
    }
  }
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
  Serial.println("connection has been established");
}
