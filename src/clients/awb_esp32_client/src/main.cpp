/* ___        _            __                _       _      __         __    ___               __
  / _ | ___  (_)_ _  ___ _/ /________  ___  (_)___  | | /| / /__  ____/ /__ / _ )___ ___  ____/ /
 / __ |/ _ \/ /  ' \/ _ `/ __/ __/ _ \/ _ \/ / __/  | |/ |/ / _ \/ __/  '_// _  / -_) _ \/ __/ _ \
/_/ |_/_//_/_/_/_/_/\_,_/\__/_/  \___/_//_/_/\__/   |__/|__/\___/_/ /_/\_\/____/\__/_//_/\__/_//_/

An desktop editor and thin clients to choreograph animatronic movements
=> here: ESP32 thin client

https://github.com/springwald
https://daniel.springwald.de

*/

#include "AwbClient.h"

// If you use more than one AWB-client, you have to enter different IDs per client here
#define CLIENT_ID 2

AwbClient awbClient = AwbClient(CLIENT_ID);

void setup()
{
  Serial.begin(115200);
  awbClient.setup();
}

void loop()
{
  awbClient.loop();
}