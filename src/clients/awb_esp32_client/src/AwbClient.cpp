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
    delay(1000);

    // set up the wlan connector
    showSetupMsg("setup wifi");
    const TCallBackErrorOccured wlanErrorOccured = [this](String message)
    { showError(message); };
    _wlanConnector = new WlanConnector(_clientId, _actualStatusInformation, wlanErrorOccured);
    _wlanConnector->setup();
    showSetupMsg("setup wifi done");

    // load the AWB project data
    _data = new AutoPlayData();

#ifdef USE_NEOPIXEL_STATUS_CONTROL
    showSetupMsg("setup neopixel");
    this->_neoPixelStatus = new NeoPixelStatusControl();
    _neoPixelStatus->setStartUpAlert(); // show alarm neopixel on startup to see unexpected restarts
    showSetupMsg("setup neopixel done");
#endif

#ifdef USE_DAC_SPEAKER
    showSetupMsg("setup dac speaker");
    this->_dacSpeaker = new DacSpeaker();
    this->_dacSpeaker->begin();
    showSetupMsg("setup dac speaker done");
#endif

    showSetupMsg("setup callbacks");
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
    const TCallBackErrorOccured mp3PlayerErrorOccured = [this](String message)
    { showError(message); };
    const TCallBackMessageToShow mp3PlayerMessageToShow = [this](String message)
    { showMsg(message); };
    const TCallBackErrorOccured autoPlayerErrorOccured = [this](String message)
    { showError(message); };
    const TCallBackErrorOccured inputManagerErrorOccured = [this](String message)
    { showError(message); };
    showSetupMsg("setup callbacks done");

    // set up the actuators

#ifdef USE_STS_SERVO

#ifdef USE_SCS_SERVO
    if (STS_SERVO_RXD == SCS_SERVO_RXD || STS_SERVO_RXD == SCS_SERVO_TXD || STS_SERVO_TXD == SCS_SERVO_RXD || STS_SERVO_TXD == SCS_SERVO_TXD)
    {
        showError("STS and SCS use the same RXD/TXD pins!");
        delay(2000);
        throw "STS and SCS use the same RXD/TXD pins!";
    }
#endif

    showSetupMsg("setup STS servos");
    this->_stSerialServoManager = new StSerialServoManager(_actualStatusInformation->stsServoValues, false, stsServoErrorOccured, STS_SERVO_RXD, STS_SERVO_TXD, STS_SERVO_SPEED, STS_SERVO_ACC);
    this->_stSerialServoManager->setup();
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
    showSetupMsg("setup STS servos done");
#endif

#ifdef USE_SCS_SERVO
    showSetupMsg("setup SCS servos");
    this->_scSerialServoManager = new StSerialServoManager(_actualStatusInformation->scsServoValues, true, scsServoErrorOccured, SCS_SERVO_RXD, SCS_SERVO_TXD, SCS_SERVO_SPEED, SCS_SERVO_ACC);
    this->_scSerialServoManager->setup();
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
    showSetupMsg("setup STS servos done");
#endif

    showMsg("Found " + String(this->_stSerialServoManager == NULL ? 0 : this->_stSerialServoManager->servoIds->size()) + " STS / " + String(this->_scSerialServoManager == NULL ? 0 : this->_scSerialServoManager->servoIds->size()) + " SCS");
    delay(1000);

#ifdef USE_PCA9685_PWM_SERVO
    showSetupMsg("setup PCA9685 PWM servos");
    this->_pca9685pwmManager = new Pca9685PwmManager(_actualStatusInformation->pwmServoValues, pca9685PwmErrorOccured, pca9685PwmMessageToShow, PCA9685_I2C_ADDRESS, PCA9685_OSC_FREQUENCY);
#endif

#ifdef USE_MP3_PLAYER_YX5300
    showSetupMsg("setup mp3 player YX5300");
    this->_mp3Player = new Mp3PlayerYX5300Manager(MP3_PLAYER_YX5300_RXD, MP3_PLAYER_YX5300_TXD, mp3PlayerErrorOccured, mp3PlayerMessageToShow);
#endif

#ifdef AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL
    auto autoPlayerStateSelectorStsServoChannel = AUTOPLAY_STATE_SELECTOR_STS_SERVO_CHANNEL;
    showSetupMsg("AutoPlayer StateSelector StsServoChannel: " + String(autoPlayerStateSelectorStsServoChannel));
#else
    auto autoPlayerStateSelectorStsServoChannel = -1;
#endif

    showSetupMsg("setup input manager");
    _inputManager = new InputManager(_data, inputManagerErrorOccured);

    showSetupMsg("setup autoplay");
    _autoPlayer = new AutoPlayer(_data, _stSerialServoManager, _scSerialServoManager, _pca9685pwmManager, _mp3Player, _inputManager, autoPlayerStateSelectorStsServoChannel, autoPlayerErrorOccured);

    // set up the packet sender receiver to receive packets from the Animatronic Workbench Studio
    showSetupMsg("setup AWB studio packet receiver");
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

    if (this->_dacSpeaker != NULL)
    {
        showSetupMsg("init dac speaker");
        this->_dacSpeaker->setVolume(1);
        this->_dacSpeaker->playIntro();
        this->_dacSpeaker->setVolume(DEFAULT_VOLUME);
    }

    showMsg("Welcome! Animatronic WorkBench ESP32 Client");
    delay(1000);
}

/**
 * show an error message on the display, neopixels and/or speaker
 */
void AwbClient::showError(String message)
{
    int durationMs = 2000;
    _display.draw_message(message, durationMs, MSG_TYPE_ERROR);

    if (_wlanConnector != NULL)
        _wlanConnector->logError(message);

    if (_neoPixelStatus != NULL)
        _neoPixelStatus->setState(NeoPixelStatusControl::STATE_ALARM, durationMs);

    if (_dacSpeaker != NULL)
        _dacSpeaker->beep();
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
 * show an info message on the display (no error)
 */
void AwbClient::showSetupMsg(String message)
{
    int durationMs = 200;
    _display.draw_message(message, durationMs, MSG_TYPE_INFO);
    if (_wlanConnector != NULL) // check if wlan connector is instanciated
        _wlanConnector->logInfo(message);
    delay(durationMs);
}

/**
 * the main loop of the AWB client
 */
void AwbClient::loop()
{
    // receive packets
    bool packetReceived = this->_packetSenderReceiver->loop();

    _wlanConnector->update();

    //_mp3Player->playSound(1);

    // update autoplay timelines and actuators
    auto criticalTemp = false;
    if (this->_stSerialServoManager != NULL)
        criticalTemp = this->_stSerialServoManager->servoCriticalTemp;
    if (this->_scSerialServoManager != NULL)
        criticalTemp = criticalTemp || this->_scSerialServoManager->servoCriticalTemp;

    if (_wlanConnector->timelineNameToPlay != NULL && _wlanConnector->timelineNameToPlay->length() > 0)
    {
        // a timeline was received via wifi from a remote control
        _autoPlayer->startNewTimelineByName(_wlanConnector->timelineNameToPlay->c_str());
        _wlanConnector->timelineNameToPlay = NULL;
    }

    _autoPlayer->update(criticalTemp);

    if (_autoPlayer->getStateSelectorStsServoChannel() != _lastAutoPlaySelectedStateId)
    {
        // an other timeline filter state was selected
        _lastAutoPlaySelectedStateId = _autoPlayer->getStateSelectorStsServoChannel();
        _display.set_debugStatus("StateId:" + String(_lastAutoPlaySelectedStateId));
    }

    if (!_autoPlayer->getCurrentTimelineName().equals(_lastAutoPlayTimelineName))
    {
        // an other timeline was started
        _lastAutoPlayTimelineName = _autoPlayer->getCurrentTimelineName();
        if (!_autoPlayer->isPlaying())
        {
            // no timeline is playing, so turn off torque for all sts servos
            if (this->_stSerialServoManager != NULL)
            {
                for (int i = 0; i < this->_stSerialServoManager->servoIds->size(); i++)
                {
                    // turn off torque for all sts servos
                    int id = this->_stSerialServoManager->servoIds->at(i);
                    this->_stSerialServoManager->setTorque(id, false);
                }
            }
            if (this->_scSerialServoManager != NULL)
            {
                for (int i = 0; i < this->_scSerialServoManager->servoIds->size(); i++)
                {
                    // turn off torque for all scs servos
                    int id = this->_scSerialServoManager->servoIds->at(i);
                    this->_scSerialServoManager->setTorque(id, false);
                }
            }
        }
        _display.set_debugStatus("Timeline:" + String(_lastAutoPlayTimelineName));
    }

    bool onlyLoadCheck = false;

    if (!packetReceived && millis() > _lastStatusMillis + 100) // update status every 100ms
    {
        if (criticalTemp == true)
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

    if (_neoPixelStatus != NULL && !packetReceived)
        _neoPixelStatus->update();

    if (_dacSpeaker != NULL)
        _dacSpeaker->update();

    // update display
    if (!packetReceived)
        _display.loop();

    // collect all status information for lcd display and WLAN status display
    _wlanConnector->memoryInfo = &_display.memoryInfo;
    _actualStatusInformation->autoPlayerCurrentStateName = _autoPlayer->getCurrentTimelineName();
    _actualStatusInformation->autoPlayerSelectedStateId = _autoPlayer->selectedStateIdFromStsServoSelector();
    _actualStatusInformation->autoPlayerIsPlaying = _autoPlayer->isPlaying();
    _actualStatusInformation->autoPlayerStateSelectorAvailable = _autoPlayer->getStateSelectorAvailable();
    _actualStatusInformation->autoPlayerCurrentTimelineName = _autoPlayer->getCurrentTimelineName();
    _actualStatusInformation->autoPlayerStateSelectorStsServoChannel = _autoPlayer->getStateSelectorStsServoChannel();
    _actualStatusInformation->activeTimelineStateIdsByInput = _autoPlayer->getStatesDebugInfo();
    _actualStatusInformation->inputStates = _inputManager->getDebugInfo();

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
            if (this->_pca9685pwmManager == NULL)
            {
                showError("Pca9685Pwm not configured!");
                return;
            }
            int channel = servos[i]["Ch"];
            int value = servos[i]["TVal"];
            String name = servos[i]["Name"];
            // store the method showMsg in an anonymous function
            _pca9685pwmManager->setTargetValue(channel, value, name);
        }
    }
    if (this->_pca9685pwmManager != NULL)
        _pca9685pwmManager->updateActuators();

    if (jsondoc.containsKey("STS")) // package contains STS bus servo data
    {
        JsonArray servos = jsondoc["STS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_stSerialServoManager == NULL)
            {
                showError("STS not configured!");
                return;
            }

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
    if (this->_stSerialServoManager != NULL)
        _stSerialServoManager->updateActuators();

    if (jsondoc.containsKey("SCS")) // package contains SCS bus servo data
    {
        JsonArray servos = jsondoc["SCS"]["Servos"];
        int stsCount = 0;
        for (size_t i = 0; i < servos.size(); i++)
        {
            if (this->_scSerialServoManager == NULL)
            {
                showError("SCS not configured!");
                return;
            }
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
    if (this->_scSerialServoManager != NULL)
        _scSerialServoManager->updateActuators();

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
    if (this->_stSerialServoManager != NULL)
        this->readStsScsServoStatuses(this->_stSerialServoManager, this->_actualStatusInformation->stsServoValues, false);

    // check Scs serial bus servos
    if (this->_scSerialServoManager != NULL)
        this->readStsScsServoStatuses(this->_scSerialServoManager, this->_actualStatusInformation->scsServoValues, true);

    // check PWM servos
    if (this->_pca9685pwmManager != NULL)
    {
        for (int i = 0; i < this->_actualStatusInformation->pwmServoValues->size(); i++)
        {
            // PWM servo driver
            if (this->_actualStatusInformation->pwmServoValues->at(i).name.length() > 0)
            {
                // PWM Servo support no status information reading
            }
        }
    }
}

void AwbClient::readStsScsServoStatuses(StSerialServoManager *serialServoManager, std::vector<ActuatorValue> *servoValues, bool isScsServo)
{
    bool criticalTemp = false;
    bool criticalLoad = false;

    // Scs serial bus servos
    for (int i = 0; i < servoValues->size(); i++)
    {
        if (servoValues->at(i).name.length() > 0)
        {
            servoValues->at(i).temperature = serialServoManager->readTemperature(servoValues->at(i).id);
            if (servoValues->at(i).temperature > (isScsServo ? SCS_SERVO_MAX_TEMPERATURE : STS_SERVO_MAX_TEMPERATURE))
            {
                criticalTemp = true;
                serialServoManager->setTorque(servoValues->at(i).id, false);
                showError("Servo " + String(servoValues->at(i).id) + " critical temperature! " + String(servoValues->at(i).temperature) + "C");
            }
            servoValues->at(i).load = serialServoManager->readLoad(servoValues->at(i).id);
            if (abs(servoValues->at(i).load) > (isScsServo ? SCS_SERVO_MAX_LOAD : STS_SERVO_MAX_LOAD))
            {
                criticalLoad = true;
                serialServoManager->setTorque(servoValues->at(i).id, false);
                showError("Servo " + String(servoValues->at(i).id) + " critical load! " + String(servoValues->at(i).load));
            }
        }
    }
    serialServoManager->servoCriticalTemp = criticalTemp;
    serialServoManager->servoCriticalLoad = criticalLoad;
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
