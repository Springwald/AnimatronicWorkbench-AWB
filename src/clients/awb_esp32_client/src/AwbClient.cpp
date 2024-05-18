#include <Arduino.h>
#include "AwbDisplay.h"
#include "Packet.h"
#include "PacketSenderReceiver.h"
#include "PacketProcessor.h"
#include <ArduinoJson.h>
#include "WlanConnector.h"
#include "AwbClient.h"

using TCallBackPacketReceived = std::function<void(unsigned int, String)>;
using TCallBackErrorOccured = std::function<void(String)>;
using TCallBackMessageToShow = std::function<void(String)>;                  // message, duration in ms (0=auto duration)
using TCallBackMessageToShowWithDuration = std::function<void(String, int)>; // message, duration in ms (0=auto duration)

#define DEFAULT_VOLUME 5

/**
 * initialize the AWB client
 */
void AwbClient::setup()
{
    _display.setup(_clientId); // set up the display

    // set up the debugging
    _debugging = new Debugging(&_display);

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
    _projectData = new ProjectData();

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
    const TCallBackErrorOccured packetProcessorErrorOccured = [this](String message)
    { showError(message); };
    const TCallBackMessageToShowWithDuration packetProcessorMessageToShow = [this](String message, int duration)
    { showMsgWithDuration(message, duration); };
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
        delay(5000);
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
    delay(_debugging->isDebugging() ? 1000 : 100);

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
            this->_packetProcessor->processPacket(payload);
        }
    };
    char *packetHeader = (char *)"AWB";
    this->_packetSenderReceiver = new PacketSenderReceiver(this->_clientId, packetHeader, packetReceived, packetErrorOccured);

    // setup the packet processor to process packets from the Animatronic Workbench Studio
    showSetupMsg("setup AWB studio packet processor");
    this->_packetProcessor = new PacketProcessor(_projectData, _stSerialServoManager, _scSerialServoManager, _pca9685pwmManager, _mp3Player, _inputManager, &_display, packetProcessorErrorOccured, packetProcessorMessageToShow);

    if (this->_dacSpeaker != NULL)
    {
        showSetupMsg("init dac speaker");
        this->_dacSpeaker->setVolume(1);
        this->_dacSpeaker->playIntro();
        this->_dacSpeaker->setVolume(DEFAULT_VOLUME);
    }

    showMsg("Welcome! Animatronic WorkBench ESP32 Client");
    delay(_debugging->isDebugging() ? 1000 : 100);
}

/**
 * show an error message on the display, neopixels and/or speaker
 */
void AwbClient::showError(String message)
{
    int durationMs = _debugging->isDebugging() ? 3000 : 2000;
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
void AwbClient::showMsgWithDuration(String message, int durationMs)
{
    if (durationMs <= 0)
        durationMs = _debugging->isDebugging() ? 1000 : 100;
    _display.draw_message(message, durationMs, MSG_TYPE_INFO);
    _wlanConnector->logInfo(message);
}

/**
 * show an info message on the display (no error)
 */
void AwbClient::showMsg(String message)
{
    this->showMsgWithDuration(message, 0);
}

/**
 * show an info message on the display (no error)
 */
void AwbClient::showSetupMsg(String message)
{
    int durationMs = _debugging->isDebugging() ? 200 : 50;
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
    _debugging->setState(Debugging::MJ_LOOP, 0);

    if (false)
    {
        // set true to test the mp3 player
        if (_mp3Player->playSound(10) == true)
        {
        }
        else
        {
            delay(1000);
            _mp3Player->playSound(1);
        }
        delay(1000);
        return;
    }

    // receive packets
    bool packetReceived = this->_packetSenderReceiver->loop();
    if (packetReceived)
        _autoPlayer->stopBecauseOfIncommingPackage();

    _debugging->setState(Debugging::MJ_LOOP, 5);

    _wlanConnector->update(_debugging->isDebugging());

    _debugging->setState(Debugging::MJ_LOOP, 10);

    //_mp3Player->playSound(1);

    // update autoplay timelines and actuators
    auto criticalTemp = false;
    if (this->_stSerialServoManager != NULL)
        criticalTemp = this->_stSerialServoManager->servoCriticalTemp;
    if (this->_scSerialServoManager != NULL)
        criticalTemp = criticalTemp || this->_scSerialServoManager->servoCriticalTemp;

    _debugging->setState(Debugging::MJ_LOOP, 15);

    if (_wlanConnector->timelineNameToPlay != NULL && _wlanConnector->timelineNameToPlay->length() > 0)
    {
        // a timeline was received via wifi from a remote control
        _autoPlayer->startNewTimelineByName(_wlanConnector->timelineNameToPlay->c_str());
        _wlanConnector->timelineNameToPlay = NULL;
    }

    _debugging->setState(Debugging::MJ_LOOP, 20);

    _autoPlayer->update(criticalTemp);

    _debugging->setState(Debugging::MJ_LOOP, 25);

    if (_autoPlayer->getStateSelectorStsServoChannel() != _lastAutoPlaySelectedStateId)
    {
        // an other timeline filter state was selected
        _lastAutoPlaySelectedStateId = _autoPlayer->getStateSelectorStsServoChannel();
        _display.set_debugStatus("StateId:" + String(_lastAutoPlaySelectedStateId));
    }

    _debugging->setState(Debugging::MJ_LOOP, 30);

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

    _debugging->setState(Debugging::MJ_LOOP, 35);

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

    _debugging->setState(Debugging::MJ_LOOP, 50);

    if (_neoPixelStatus != NULL && !packetReceived)
        _neoPixelStatus->update();

    _debugging->setState(Debugging::MJ_LOOP, 55);

    if (_dacSpeaker != NULL)
        _dacSpeaker->update();

    _debugging->setState(Debugging::MJ_LOOP, 60);

    // update display
    if (!packetReceived)
        _display.loop();

    _debugging->setState(Debugging::MJ_LOOP, 65);

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
    _actualStatusInformation->lastSoundPlayed = _autoPlayer->getLastSoundPlayed();

    _debugging->setState(Debugging::MJ_LOOP, 70);

    if (millis() - _startMillis < 5000)
    {
        _display.resetDebugInfos(); // only check memory usage after 5 seconds to avoid false alarms when starting up
    }

    _debugging->setState(Debugging::MJ_LOOP, 99);
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
