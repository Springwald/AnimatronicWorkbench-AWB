#include <Arduino.h>
#include "AwbDisplay.h"
#include "Packet.h"
#include "PacketSenderReceiver.h"
#include <ArduinoJson.h>
#include "WlanConnector.h"
#include "AwbClient.h"

bool demoMode = false;

using TCallBackPacketReceived = std::function<void(unsigned int, String)>;
using TCallBackErrorOccured = std::function<void(String)>;
using TCallBackMessageToShow = std::function<void(String)>;

StaticJsonDocument<1024 * 32> jsondoc;

#define DEFAULT_VOLUME 5

/**
 * initialize the AWB client
 */
void AwbClient::setup()
{
    _display.setup(_clientId); // set up the display

    // set up the wlan connector
    const TCallBackErrorOccured wlanErrorOccured = [this](String message)
    { showError(message); };
    _wlanConnector = new WlanConnector(_clientId, _actualStatusInformation, wlanErrorOccured);
    _wlanConnector->setup();

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    this->_neoPixelStatus = new NeoPixelStatusControl();
    _neoPixelStatus->setStartUpAlert(); // show alarm neopixel on startup to see unexpected restarts
#endif

#ifdef USE_DAC_SPEAKER
    this->_dacSpeaker = DacSpeaker();
    this->_dacSpeaker.begin();
#endif

    // set up error callbacks for the different components
    const TCallBackErrorOccured packetErrorOccured = [this](String message)
    { showError(message); };
    const TCallBackErrorOccured pca9685PwmErrorOccured = [this](String message)
    { showError(message); };
    const TCallBackMessageToShow pca9685PwmMessageToShow = [this](String message)
    { showMsg(message); };
    const TCallBackErrorOccured stsServoErrorOccured = [this](String message)
    { showError(message); };
    const TCallBackErrorOccured scsServoErrorOccured = [this](String message)
    { showError(message); };
    const TCallBackErrorOccured autoPlayerErrorOccured = [this](String message)
    { showError(message); };

    // set up the actuators
    this->_stSerialServoManager = new StSerialServoManager(_actualStatusInformation->stsServoValues, false, stsServoErrorOccured, STS_SERVO_RXD, STS_SERVO_TXD, STS_SERVO_SPEED, STS_SERVO_ACC);
    this->_stSerialServoManager->setup();

    this->_scSerialServoManager = new StSerialServoManager(_actualStatusInformation->scsServoValues, true, scsServoErrorOccured, SCS_SERVO_RXD, SCS_SERVO_TXD, SCS_SERVO_SPEED, SCS_SERVO_ACC);
    this->_scSerialServoManager->setup();

    this->_pca9685pwmManager = new Pca9685PwmManager(_actualStatusInformation->pwmServoValues, pca9685PwmErrorOccured, pca9685PwmMessageToShow, PCA9685_I2C_ADDRESS, PCA9685_SPEED, PCA9685_ACC);

    // iterate through all stsServoIds
    for (int i = 0; i < this->_stSerialServoManager->servoIds->size(); i++)
    {
        // get the id of stSerialServoManager servoIds
        int id = this->_stSerialServoManager->servoIds->at(i);
        auto actuatorValue = ActuatorValue();
        actuatorValue.id = id;
        actuatorValue.targetValue = -1;
        actuatorValue.name = "no set yet.";
        this->_actualStatusInformation->stsServoValues->push_back(actuatorValue);
    }

    // iterate through all scsServoIds
    for (int i = 0; i < this->_scSerialServoManager->servoIds->size(); i++)
    {
        // get the id of scSerialServoManager servoIds
        int id = this->_scSerialServoManager->servoIds->at(i);
        auto actuatorValue = ActuatorValue();
        actuatorValue.id = id;
        actuatorValue.targetValue = -1;
        actuatorValue.name = "no set yet.";
        this->_actualStatusInformation->scsServoValues->push_back(actuatorValue);
    }

    showMsg("Found " + String(this->_stSerialServoManager->servoIds->size()) + " STS / " + String(this->_scSerialServoManager->servoIds->size()) + " SCS servos");
    delay(1000);

#ifdef AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL
    auto autoPlayerStateSelectorStsServoChannel = AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL;
#else
    auto autoPlayerStateSelectorStsServoChannel = -1;
#endif

    _autoPlayer = new AutoPlayer(_stSerialServoManager, _scSerialServoManager, _pca9685pwmManager, autoPlayerStateSelectorStsServoChannel, autoPlayerErrorOccured);

    // set up the packet sender receiver to receive packets from the Animatronic Workbench Studio
    const TCallBackPacketReceived packetReceived = [this](unsigned int clientId, String payload)
    {
        // process the packet
        if (clientId == this->_clientId)
        {
            this->processPacket(payload);
        }
    };
    char *packetHeader = (char *)"AWB";
    this->_packetSenderReceiver = new PacketSenderReceiver(this->_clientId, packetHeader, packetReceived, packetErrorOccured);

#ifdef USE_DAC_SPEAKER
    this->_dacSpeaker.setVolume(1);
    this->_dacSpeaker.playIntro();
    this->_dacSpeaker.setVolume(DEFAULT_VOLUME);
#endif

    showMsg("Welcome! Animatronic WorkBench ESP32 Client");
}

/**
 * show an error message on the display, neopixels and/or speaker
 */
void AwbClient::showError(String message)
{
    int durationMs = 2000;
    _display.draw_message(message, durationMs, MSG_TYPE_ERROR);
    _wlanConnector->logError(message);
#ifdef USE_NEOPIXEL_STATUS_CONTROL
    _neoPixelStatus->setState(NeoPixelStatusControl::STATE_ALARM, durationMs);
#endif
#ifdef USE_DAC_SPEAKER
    _dacSpeaker.beep();
#endif
}

/**
 * show an info message on the display (no error)
 */
void AwbClient::showMsg(String message)
{
    int durationMs = 1000;
    _display.draw_message(message, durationMs, MSG_TYPE_INFO);
    _wlanConnector->logInfo(message);
}

/**
 * the main loop of the AWB client
 */
void AwbClient::loop()
{

    // receive packets
    bool packetReceived = this->_packetSenderReceiver->loop();

    // update autoplay timelines and actuators
    _autoPlayer->update(this->_stSerialServoManager->servoCriticalTemp);
    if (_autoPlayer->selectedStateId() != _lastAutoPlaySelectedStateId)
    {
        // an other timeline filter state was selected
        _lastAutoPlaySelectedStateId = _autoPlayer->selectedStateId();
        _display.set_debugStatus("StateId:" + String(_lastAutoPlaySelectedStateId));
    }
    if (!_autoPlayer->getCurrentTimelineName().equals(_lastAutoPlayTimelineName))
    {
        // an other timeline was started
        _lastAutoPlayTimelineName = _autoPlayer->getCurrentTimelineName();
        if (!_autoPlayer->isPlaying())
        {
            // no timeline is playing, so turn off torque for all sts servos
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

    if (!packetReceived && millis() > _lastStatusMillis + 100) // update status every 100ms
    {
        if (this->_stSerialServoManager->servoCriticalTemp == true || this->_stSerialServoManager->servoCriticalLoad == true)
        {
            // critical temperature or load detected, so only check and show load status
            readActuatorsStatuses();
            showTemperaturStatuses();
        }
        else
        {
            // no critical temperature or load detected
            _lastStatusMillis = millis();
            _displayStateCounter++;

            if (onlyLoadCheck)
            {
                // only check and show load status
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

    // collect all status information for lcd display and WLAN status display
    _wlanConnector->memoryInfo = &_display.memoryInfo;
    _actualStatusInformation->autoPlayerCurrentStateName = _autoPlayer->getCurrentTimelineName();
    _actualStatusInformation->autoPlayerSelectedStateId = _autoPlayer->selectedStateId();
    _actualStatusInformation->autoPlayerIsPlaying = _autoPlayer->isPlaying();
    _actualStatusInformation->autoPlayerSelectedStateId = _autoPlayer->selectedStateId();
    _actualStatusInformation->autoPlayerStateSelectorAvailable = _autoPlayer->getStateSelectorAvailable();
    _actualStatusInformation->autoPlayerCurrentTimelineName = _autoPlayer->getCurrentTimelineName();
    _actualStatusInformation->autoPlayerStateSelectorStsServoChannel = _autoPlayer->getStateSelectorStsServoChannel();
    _wlanConnector->update();

    if (millis() - _startMillis < 5000)
    {
        _display.resetDebugInfos(); // only check memory usage after 5 seconds to avoid false alarms when starting up
    }
}

/**
 * process a received packet from the Animatronic Workbench Studio
 */
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
        // packet content is not valid json
        showError("json:" + String(error.c_str()));
        return;
    }

    if (jsondoc.containsKey("DispMsg")) // packat contains a display message
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
            showMsg(message);
        }
    }

    if (jsondoc.containsKey("Pca9685Pwm")) // packet contains Pca9685 PWM driver data
    {
        JsonArray servos = jsondoc["Pca9685Pwm"]["Servos"];
        for (size_t i = 0; i < servos.size(); i++)
        {
            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];
            // store the method showMsg in an anonymous function
            _pca9685pwmManager->setTargetValue(channel, value, name);
        }
    }
    _pca9685pwmManager->updateActuators();

    if (jsondoc.containsKey("STS")) // package contains STS bus servo data
    {
        JsonArray servos = jsondoc["STS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            int id = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];

            bool done = false;
            for (int f = 0; f < this->_actualStatusInformation->stsServoValues->size(); f++)
            {
                if (this->_actualStatusInformation->stsServoValues->at(f).id == id)
                {
                    // set servo target value
                    this->_actualStatusInformation->stsServoValues->at(f).id = id;
                    this->_actualStatusInformation->stsServoValues->at(f).targetValue = value;
                    this->_actualStatusInformation->stsServoValues->at(f).name = name;
                    done = true;
                    break;
                }
            }
            if (!done)
                showError("STS Servo " + String(id) + " not attached!");
        }
    }
    _stSerialServoManager->updateActuators();

    if (jsondoc.containsKey("SCS")) // package contains STS bus servo data
    {
        JsonArray servos = jsondoc["SCS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            int id = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];

            bool done = false;
            for (int f = 0; f < this->_actualStatusInformation->scsServoValues->size(); f++)
            {
                if (this->_actualStatusInformation->scsServoValues->at(f).id == id)
                {
                    // set servo target value
                    this->_actualStatusInformation->scsServoValues->at(f).id = id;
                    this->_actualStatusInformation->scsServoValues->at(f).targetValue = value;
                    this->_actualStatusInformation->scsServoValues->at(f).name = name;
                    done = true;
                    break;
                }
            }
            if (!done)
                showError("SCS Servo " + String(id) + " not attached!");
        }
    }
    _stSerialServoManager->updateActuators();

    _autoPlayer->stopBecauseOfIncommingPackage();

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    _neoPixelStatus->showActivity();
#endif
}

/**
 * read the status information from the actuators
 */
void AwbClient::readActuatorsStatuses()
{
    // check Sts serial bus servos
    this->readStsScsServoStatuses(this->_stSerialServoManager, this->_actualStatusInformation->stsServoValues);
    // check Scs serial bus servos
    this->readStsScsServoStatuses(this->_scSerialServoManager, this->_actualStatusInformation->scsServoValues);

    // check PWM servos
    for (int i = 0; i < this->_actualStatusInformation->pwmServoValues->size(); i++)
    {
        // PWM servo driver
        if (this->_actualStatusInformation->pwmServoValues->at(i).name.length() > 0)
        {
            // PWM Servo support no status information reading
        }
    }
}

void AwbClient::readStsScsServoStatuses(StSerialServoManager *_serialServoManager, std::vector<ActuatorValue> *servoValues)
{
    bool criticalTemp = false;
    bool criticalLoad = false;

    // Scs serial bus servos
    for (int i = 0; i < servoValues->size(); i++)
    {
        if (servoValues->at(i).name.length() > 0)
        {
            servoValues->at(i).temperature = _stSerialServoManager->readTemperature(servoValues->at(i).id);
            if (this->_actualStatusInformation->stsServoValues->at(i).temperature > STS_SERVO_MAX_TEMPERATURE)
            {
                criticalTemp = true;
                _stSerialServoManager->setTorque(servoValues->at(i).id, false);
                showError("Servo " + String(servoValues->at(i).id) + " critical temperature! " + String(servoValues->at(i).temperature) + "C");
            }
            servoValues->at(i).load = _stSerialServoManager->readLoad(servoValues->at(i).id);
            if (abs(servoValues->at(i).load) > STS_SERVO_MAX_LOAD)
            {
                criticalLoad = true;
                _stSerialServoManager->setTorque(servoValues->at(i).id, false);
                showError("Servo " + String(servoValues->at(i).id) + " critical load! " + String(servoValues->at(i).load));
            }
        }
    }
    _serialServoManager->servoCriticalTemp = criticalTemp;
    _serialServoManager->servoCriticalLoad = criticalLoad;
}

/**
 * show the target values of the actuators on the display
 */
void AwbClient::showValues()
{
    String values[100]; //_stsServoValues->size()];
    int used = 0;
    for (int i = 0; i < this->_actualStatusInformation->stsServoValues->size(); i++)
    {
        // sts serial bus servos
        if (this->_actualStatusInformation->stsServoValues->at(i).name.length() > 0)
        {
            values[used] = String(this->_actualStatusInformation->stsServoValues->at(i).id) + ":" + this->_actualStatusInformation->stsServoValues->at(i).targetValue; // use ids only (needs less space)
            used++;
        }
    }
    for (int i = 0; i < this->_actualStatusInformation->scsServoValues->size(); i++)
    {
        // scs serial bus servos
        if (this->_actualStatusInformation->scsServoValues->at(i).name.length() > 0)
        {
            values[used] = String(this->_actualStatusInformation->scsServoValues->at(i).id) + ":" + this->_actualStatusInformation->scsServoValues->at(i).targetValue; // use ids only (needs less space)
            used++;
        }
    }
    for (int i = 0; i < this->_actualStatusInformation->pwmServoValues->size(); i++)
    {
        // Adafruit PWM servo driver
        if (this->_actualStatusInformation->pwmServoValues->at(i).name.length() > 0)
        {
            values[used] = "P" + this->_actualStatusInformation->pwmServoValues->at(i).name + ":" + this->_actualStatusInformation->pwmServoValues->at(i).targetValue;
            used++;
        }
    }
    _display.set_values(values, used);
}

/**
 * show the temperature values of the actuators on the display
 */
void AwbClient::showTemperaturStatuses()
{
    int maxValues = this->_actualStatusInformation->stsServoValues->size() + this->_actualStatusInformation->pwmServoValues->size();
    String statuses[maxValues];
    int used = 0;
    for (int i = 0; i < this->_actualStatusInformation->stsServoValues->size(); i++)
    {
        // sts serial bus servos
        if (this->_actualStatusInformation->stsServoValues->at(i).name.length() > 0 && used < maxValues)
        {
            statuses[used] = String(this->_actualStatusInformation->stsServoValues->at(i).id) + ":" + this->_actualStatusInformation->stsServoValues->at(i).temperature + "C";
            used++;
        }
    }
    for (int i = 0; i < this->_actualStatusInformation->scsServoValues->size(); i++)
    {
        // scs serial bus servos
        if (this->_actualStatusInformation->scsServoValues->at(i).name.length() > 0 && used < maxValues)
        {
            statuses[used] = String(this->_actualStatusInformation->scsServoValues->at(i).id) + ":" + this->_actualStatusInformation->scsServoValues->at(i).temperature + "C";
            used++;
        }
    }
    for (int i = 0; i < this->_actualStatusInformation->pwmServoValues->size(); i++)
    {
        // Adafruit PWM servo driver
        if (this->_actualStatusInformation->pwmServoValues->at(i).name.length() > 0 && used < maxValues)
        {
            // pwm servos have no status information
            statuses[used] = this->_actualStatusInformation->pwmServoValues->at(i).name + ": PWM?";
            used++;
        }
    }
    _display.set_values(statuses, used);
}

/**
 * show the load (torque) status of the actuators on the display
 */
void AwbClient::showLoadStatuses()
{
    int maxValues = this->_actualStatusInformation->stsServoValues->size() + this->_actualStatusInformation->pwmServoValues->size();
    String statuses[maxValues];
    int used = 0;
    for (int i = 0; i < maxValues; i++)
    {
        // sts serial bus servos
        if (this->_actualStatusInformation->stsServoValues->at(i).name.length() > 0 && used < maxValues)
        {
            statuses[used] = String(this->_actualStatusInformation->stsServoValues->at(i).id) + ":" + this->_actualStatusInformation->stsServoValues->at(i).load;
            used++;
        }
        // scs serial bus servos
        if (this->_actualStatusInformation->scsServoValues->at(i).name.length() > 0 && used < maxValues)
        {
            statuses[used] = String(this->_actualStatusInformation->scsServoValues->at(i).id) + ":" + this->_actualStatusInformation->scsServoValues->at(i).load;
            used++;
        }
        // Adafruit PWM servo driver
        if (this->_actualStatusInformation->pwmServoValues->at(i).name.length() > 0 && used < maxValues)
        {
            // pwm servos have no status information
            statuses[used] = this->_actualStatusInformation->pwmServoValues->at(i).name + ": PWM?";
            used++;
        }
    }
    _display.set_values(statuses, used);
}
