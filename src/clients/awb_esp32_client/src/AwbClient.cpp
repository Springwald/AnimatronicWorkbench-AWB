#include <Arduino.h>
#include "AwbDisplay.h"
#include "Packet.h"
#include "PacketSenderReceiver.h"
#include <ArduinoJson.h>
#include "AwbClient.h"

bool demoMode = false;

using TCallBackPacketReceived = std::function<void(unsigned int, String)>;
using TCallBackErrorOccured = std::function<void(String)>;

StaticJsonDocument<1024 * 32> jsondoc;

#define DEFAULT_VOLUME 5

void AwbClient::setup()
{

    _display.setup(_clientId);

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    this->_neoPixelStatus = new NeoPixelStatusControl();
    _neoPixelStatus->setStartUpAlert(); // show alarm neopixel on startup to see unexpected restarts
#endif

#ifdef USE_DAC_SPEAKER
    this->_dacSpeaker = DacSpeaker();
    this->_dacSpeaker.begin();
#endif

    const TCallBackPacketReceived packetReceived = [this](unsigned int clientId, String payload)
    {
        // process the packet
        if (clientId == this->_clientId)
        {
            this->processPacket(payload);
        }
    };

    const TCallBackErrorOccured packetErrorOccured = [this](String message)
    { showError(message); };

    const TCallBackErrorOccured adafruitPwmErrorOccured = [this](String message)
    { showError(message); };

    const TCallBackErrorOccured stsServoErrorOccured = [this](String message)
    { showError(message); };

    const TCallBackErrorOccured autoPlayerErrorOccured = [this](String message)
    { showError(message); };

    this->_adafruitpwmManager = new AdafruitPwmManager(adafruitPwmErrorOccured);
    this->_stSerialServoManager = new StSerialServoManager(stsServoErrorOccured, STS_SERVO_RXD, STS_SERVO_TXD, STS_SERVO_SPEED, STS_SERVO_ACC);
    this->_stSerialServoManager->setup();
    _autoPlayer = new AutoPlayer(_stSerialServoManager, AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL, autoPlayerErrorOccured);

    // iterate through all stsServoIds
    for (int i = 0; i < this->_stSerialServoManager->servoIds->size(); i++)
    {
        // get the first id of stSerialServoManager servoIds
        int id = this->_stSerialServoManager->servoIds->at(i);
        _stsServoValues[i].id = id;
        _stsServoValues[i].targetValue = -1;
        _stsServoValues[i].name = "no set yet.";
    }
    showValues();

    char *packetHeader = (char *)"AWB";
    this->_packetSenderReceiver = new PacketSenderReceiver(this->_clientId, packetHeader, packetReceived, packetErrorOccured);

#ifdef USE_DAC_SPEAKER
    this->_dacSpeaker.setVolume(1);
    this->_dacSpeaker.playIntro();
    this->_dacSpeaker.setVolume(DEFAULT_VOLUME);
#endif

    _display.resetDebugInfos();
    _display.draw_message("Welcome to the ESP32 Client for Animatronic WorkBench.", 2000, MSG_TYPE_INFO);
}

void AwbClient::showError(String message)
{
    int durationMs = 2000;
    _display.draw_message(message, durationMs, MSG_TYPE_ERROR);
#ifdef USE_NEOPIXEL_STATUS_CONTROL
    _neoPixelStatus->setState(NeoPixelStatusControl::STATE_ALARM, durationMs);
#endif
#ifdef USE_DAC_SPEAKER
    _dacSpeaker.beep();
#endif
}

void AwbClient::loop()
{
    // receive packets
    bool packetReceived = this->_packetSenderReceiver->loop();

    _autoPlayer->update(_servoCriticalTemp);

    if (_autoPlayer->selectedStateId() != _lastAutoPlaySelectedStateId)
    {
        _lastAutoPlaySelectedStateId = _autoPlayer->selectedStateId();
        _display.set_debugStatus("StateId:" + String(_lastAutoPlaySelectedStateId));
    }
    if (!_autoPlayer->getCurrentTimelineName().equals(_lastAutoPlayTimelineName))
    {
        _lastAutoPlayTimelineName = _autoPlayer->getCurrentTimelineName();
        if (!_autoPlayer->isPlaying())
        {
            for (int i = 0; i < this->_stSerialServoManager->servoIds->size(); i++)
            {
                // turn off torque for all sts servos
                int id = this->_stSerialServoManager->servoIds->at(i);
                this->_stSerialServoManager->setTorque(id, false);
            }
        }
        _display.set_debugStatus("Timeline:" + String(_lastAutoPlayTimelineName));
    }

    bool onlyLoadCheck = false;

    if (!packetReceived && millis() > _lastStatusMillis + 100)
    {
        if (_servoCriticalTemp || _servoCriticalLoad)
        {
            readActuatorsStatuses();
            showTemperaturStatuses();
        }
        else
        {
            _lastStatusMillis = millis();
            _displayStateCounter++;

            if (onlyLoadCheck)
            {
                readActuatorsStatuses();
                showLoadStatuses();
            }
            else
            {
                if (_displayStateCounter < 100)
                {
                    showValues();
                    if (_displayStateCounter == 50)
                        readActuatorsStatuses();
                }
                else if (_displayStateCounter < 150)
                {
                    showTemperaturStatuses();
                }
                else if (_displayStateCounter < 200)
                {
                    if (_displayStateCounter == 151)
                    {
                        _display.set_debugStatus_dirty();
                    }
                }
                else
                    _displayStateCounter = 0;
            }
        }
    }

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    if (!packetReceived)
        _neoPixelStatus->update();
#endif
#ifdef USE_DAC_SPEAKER
    _dacSpeaker.update();
#endif

    // update display
    if (!packetReceived)
        _display.loop();
}

void AwbClient::processPacket(String payload)
{
    if (demoMode == true)
    {
        _display.set_debugStatus("demo:" + payload); // show payload string 1:1 on display
        return;
    }

    DeserializationError error = deserializeJson(jsondoc, payload);
    if (error)
    {
        showError("json:" + String(error.c_str()));
        return;
    }

    if (jsondoc.containsKey("DispMsg")) // DisplayMessage
    {
        const char *message = jsondoc["DispMsg"]["Msg"];
        if (message == NULL)
        {
            // should not happen, instead the whole DispMsg should be missing
            _display.draw_message("DispMsg?!? " + payload, 5000, MSG_TYPE_ERROR);
        }
        else
        {
            int duration = jsondoc["DispMsg"]["Ms"];
            if (duration == 0)
                duration = 1000; // default duration
            _display.draw_message(message, duration, MSG_TYPE_INFO);
        }
    }

    if (jsondoc.containsKey("AdfPwm")) // Adafruit PWM driver
    {
        JsonArray channels = jsondoc["AdfPwm"]["Ch"];
        for (size_t i = 0; i < channels.size(); i++)
        {
            int channel = channels[i]["Ch"];
            int value = channels[i]["Val"];
            _pwmServoValues[channel].targetValue = value;
        }
    }

    if (jsondoc.containsKey("STS")) // STS bus Servos
    {
        JsonArray channels = jsondoc["STS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < channels.size(); i++)
        {
            int id = channels[i]["Ch"];
            int value = channels[i]["TVal"];
            String name = channels[i]["Name"];

            bool done = false;
            for (int f = 0; f < maxActuatorValues; f++)
            {
                if (_stsServoValues[f].id == id)
                {
                    // set servo target value
                    _stsServoValues[f].id = id;
                    _stsServoValues[f].targetValue = value;
                    _stsServoValues[f].name = name;
                    done = true;
                    break;
                }
            }
            if (!done)
                showError("Servo " + String(id) + " not attached!");
        }
    }

    updateActuators();
    _autoPlayer->stopBecauseOfIncommingPackage();

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    _neoPixelStatus->showActivity();
#endif
}

void AwbClient::updateActuators()
{
    if (_servoCriticalTemp || _servoCriticalLoad)
        return;

    for (int i = 0; i < maxActuatorValues; i++)
    {
        // Sts serial bus servos
        if (_stsServoValues[i].name.length() > 0)
        {
            if (_stsServoValues[i].targetValue == -1)
            {
                // turn servo off
                _stSerialServoManager->setTorque(_stsServoValues[i].id, false);
            }
            else
            {
                // set new target value if changed
                if (_stsServoValues[i].currentValue != _stsServoValues[i].targetValue)
                {
                    _stSerialServoManager->writePosition(_stsServoValues[i].id, _stsServoValues[i].targetValue);
                    _stsServoValues[i].currentValue = _stsServoValues[i].targetValue;
                }
            }
        }

        // Adafruit PWM driver
        if (_pwmServoValues[i].name.length() > 0)
        {
            // not implemented yet
        }
    }
}

void AwbClient::readActuatorsStatuses()
{
    bool criticalTemp = false;
    bool criticalLoad = false;

    for (int i = 0; i < maxActuatorValues; i++)
    {
        // Sts serial bus servos
        if (_stsServoValues[i].name.length() > 0)
        {
            _stsServoValues[i].temperature = _stSerialServoManager->readTemperature(_stsServoValues[i].id);
            if (_stsServoValues[i].temperature > STS_SERVO_MAX_TEMPERATURE)
            {
                criticalTemp = true;
                _stSerialServoManager->setTorque(_stsServoValues[i].id, false);
                showError("Servo " + String(_stsServoValues[i].id) + " critical temperature! " + String(_stsServoValues[i].temperature) + "C");
            }
            _stsServoValues[i].load = _stSerialServoManager->readLoad(_stsServoValues[i].id);
            if (abs(_stsServoValues[i].load) > STS_SERVO_MAX_LOAD)
            {
                criticalLoad = true;
                _stSerialServoManager->setTorque(_stsServoValues[i].id, false);
                showError("Servo " + String(_stsServoValues[i].id) + " critical load! " + String(_stsServoValues[i].load));
            }
        }

        // Adafruit PWM servo driver
        if (_pwmServoValues[i].name.length() > 0)
        {
            // PWM Servo support no status information reading
        }
    }

    _servoCriticalTemp = criticalTemp;
    _servoCriticalLoad = criticalLoad;
}

void AwbClient::showValues()
{
    int maxValues = maxActuatorValues * 2;
    String values[maxValues];
    int used = 0;
    for (int i = 0; i < maxActuatorValues; i++)
    {
        // sts serial bus servos
        if (_stsServoValues[i].name.length() > 0 && used < maxValues)
        {
            // values[used] = _stsServoValues[i].name + ":" + _stsServoValues[i].targetValue; // use names
            values[used] = String(_stsServoValues[i].id) + ":" + _stsServoValues[i].targetValue; // use ids only (needs less space)
            used++;
        }

        // Adafruit PWM servo driver
        if (_pwmServoValues[i].name.length() > 0 && used < maxValues)
        {
            values[used] = "P" + _pwmServoValues[i].name + ":" + _pwmServoValues[i].targetValue;
            used++;
        }
    }
    _display.set_values(values, used);
}

void AwbClient::showTemperaturStatuses()
{
    int maxValues = maxActuatorValues * 2;
    String statuses[maxValues];
    int used = 0;
    for (int i = 0; i < maxActuatorValues; i++)
    {
        // sts serial bus servos
        if (_stsServoValues[i].name.length() > 0 && used < maxValues)
        {
            statuses[used] = String(_stsServoValues[i].id) + ":" + _stsServoValues[i].temperature + "C";
            used++;
        }
        // Adafruit PWM servo driver
        if (_pwmServoValues[i].name.length() > 0 && used < maxValues)
        {
            // pwm servos have no status information
            statuses[used] = _pwmServoValues[i].name + ": PWM?";
            used++;
        }
    }
    _display.set_values(statuses, used);
}

void AwbClient::showLoadStatuses()
{
    int maxValues = maxActuatorValues * 2;
    String statuses[maxValues];
    int used = 0;
    for (int i = 0; i < maxActuatorValues; i++)
    {
        // sts serial bus servos
        if (_stsServoValues[i].name.length() > 0 && used < maxValues)
        {
            statuses[used] = String(_stsServoValues[i].id) + ":" + _stsServoValues[i].load;
            used++;
        }
        // Adafruit PWM servo driver
        if (_pwmServoValues[i].name.length() > 0 && used < maxValues)
        {
            // pwm servos have no status information
            statuses[used] = _pwmServoValues[i].name + ": PWM?";
            used++;
        }
    }
    _display.set_values(statuses, used);
}
