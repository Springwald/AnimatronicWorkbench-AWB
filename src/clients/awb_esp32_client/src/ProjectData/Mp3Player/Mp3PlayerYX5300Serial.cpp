
#include <Arduino.h>
#include "Mp3PlayerYX5300Serial.h"

bool Mp3PlayerYX5300Serial::playSound(int trackNo)
{
    _mp3->playTrack(trackNo);
    auto status = _mp3->getStatus();

    switch (status->code)
    {
    case MD_YX5300::STS_OK:
        //_errorOccured("Mp3PlayerYX5300Manager status: STS_OK");
        return true;

    case MD_YX5300::STS_PLAYING:
        //_errorOccured("Mp3PlayerYX5300Manager status: STS_PLAYING");
        return true;

    case MD_YX5300::STS_TIMEOUT:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_TIMEOUT");
        break;
    case MD_YX5300::STS_VERSION:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_VERSION");
        break;
    case MD_YX5300::STS_CHECKSUM:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_CHECKSUM");
        break;
    case MD_YX5300::STS_TF_INSERT:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_TF_INSERT");
        break;
    case MD_YX5300::STS_TF_REMOVE:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_TF_REMOVE");
        break;
    case MD_YX5300::STS_ERR_FILE:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_ERR_FILE");
        break;
    case MD_YX5300::STS_ACK_OK:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_ACK_OK");
        break;
    case MD_YX5300::STS_FILE_END:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_FILE_END");
        break;
    case MD_YX5300::STS_INIT:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_INIT");
        break;
    case MD_YX5300::STS_STATUS:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_STATUS");
        break;
    case MD_YX5300::STS_EQUALIZER:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_EQUALIZER");
        break;
    case MD_YX5300::STS_VOLUME:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_VOLUME");
        break;
    case MD_YX5300::STS_TOT_FILES:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_TOT_FILES");
        break;

        break;
    case MD_YX5300::STS_FLDR_FILES:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_FLDR_FILES");
        break;
    case MD_YX5300::STS_TOT_FLDR:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_TOT_FLDR");
        break;
    default:
        _errorOccured("Mp3PlayerYX5300Manager status: STS_??? 0x" + String(status->code, HEX));
        break;
    }
    return false;
}

bool Mp3PlayerYX5300Serial::stopSound()
{
    return _mp3->playStop();
}
