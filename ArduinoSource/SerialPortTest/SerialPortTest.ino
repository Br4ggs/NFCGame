// constants
String establishedConnectionKey = "setup";
String waitForConnectionKey = "setConnection";
char endOfMessage = '#';

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
  /*if(isConnected){
    //Serial.println("IM active!!!!");
    
    if(Serial.available() > 0){
      String data = Serial.readStringUntil('\n');
      data.trim();
      Serial.println(data);
    }
  }*/
}

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
