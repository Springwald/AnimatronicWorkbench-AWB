// Animatronic WorkBench core routines
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2023 Daniel Springwald  - 44789 Bochum, Germany
// https://daniel.springwald.de - daniel@springwald.de
// All rights reserved   -  Licensed under MIT License

namespace Awb.Core.InputControllers.TimelineInputControllers
{
    public class TimelineControllerEventArgs : EventArgs
    {
        public enum EventTypes
        {
            /// <summary>
            /// Set the playpos to a new absolute position
            /// </summary>
            PlayPosAbsoluteChanged,

            /// <summary>
            /// Start the playback
            /// </summary>
            Play,

            /// <summary>
            /// stop the playback
            /// </summary>
            Stop,

            /// <summary>
            /// play mode: Faster playback
            /// </summary>
            Forward,

            /// <summary>
            /// play mode: slower playback
            /// </summary>
            Backwards,

            /// <summary>
            /// set the given actuator to a new vakue
            /// </summary>
            ActuatorValueChanged,

            /// <summary>
            /// set the given actuator to its default value
            /// </summary>
            ActuatorSetValueToDefault,

            /// <summary>
            /// set or remove a point for the given actuator
            /// </summary>
            ActuatorTogglePoint,

            /// <summary>
            /// editor mode: move timeline window some seconds forwards
            /// </summary>
            NextPage,

            /// <summary>
            /// editor mode: move timeline window some seconds backwards
            /// </summary>
            PreviousPage,

            /// <summary>
            /// editor mode: switch to the next 8 actuator bank
            /// </summary>
            NextBank,

            /// <summary>
            /// editor mode: save the actual timeline
            /// </summary>
            Save
        }

        public EventTypes EventType { get; }

        public double ValueInPercent { get; }

        /// <summary>
        /// number of acturator, zero based
        /// </summary>
        public int ActuatorIndex_ { get; }

        public TimelineControllerEventArgs(EventTypes eventType, int actuatorIndex, double valueInPercent)
        {
            EventType = eventType;
            ValueInPercent = valueInPercent;
            ActuatorIndex_ = actuatorIndex;
        }

        public TimelineControllerEventArgs(EventTypes eventType)
        {
            EventType = eventType;
            ValueInPercent = -1;
            ActuatorIndex_ = -1;
        }

    }
}
