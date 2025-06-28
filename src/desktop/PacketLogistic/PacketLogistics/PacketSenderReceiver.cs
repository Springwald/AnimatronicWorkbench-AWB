// Animatronic WorkBench
// https://github.com/Springwald/AnimatronicWorkBench-AWB
//
// (C) 2025 Daniel Springwald      -     Bochum, Germany
// https://daniel.springwald.de - segfault@springwald.de
// All rights reserved    -   Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;

namespace PacketLogistics
{
    public abstract class PacketSenderReceiver<PayloadTypes> : IDisposable where PayloadTypes : Enum
    {
        public class PacketReceivedEventArgs : EventArgs
        {
            /// <summary>
            /// The value content of the packet that was received or sent.
            /// </summary>
            public string Payload { get; }

            public PacketReceivedEventArgs(string payload)
            {
                Payload = payload;
            }
        }

        /// <summary>
        /// If an error occurs during sending or receiving packets, these arguments are passed to the ErrorOccured event.
        /// </summary>
        public class ErrorEventArgs : EventArgs
        {
            public string Message { get; }

            public ErrorEventArgs(String message)
            {
                Message = message;
            }
        }

        /// <summary>
        /// Is raised when a packet is received.
        /// </summary>
        public event EventHandler<PacketReceivedEventArgs>? PacketReceived;

        /// <summary>
        /// Is raised when an error is encountered sending or receiving packets.
        /// </summary>
        public event EventHandler<ErrorEventArgs>? ErrorOccured;


        /// <summary>
        /// The possible states of the PacketSenderReceiver.
        /// </summary>
        public enum States
        {
            /// <summary>
            /// The PacketSenderReceiver has not yet started connecting.
            /// </summary>
            NotStarted,

            /// <summary>
            /// The PacketSenderReceiver is currently trying to connect to the device.
            /// </summary>
            Connecting,

            /// <summary>
            /// The PacketSenderReceiver is connected and ready to send or receive packets.
            /// </summary>
            Idle,

            /// <summary>
            /// The PacketSenderReceiver is waiting for an answer after sending a packet.
            /// </summary>
            WaitingForAnswer
        }

        /// <summary>
        /// How long to wait for a response after sending a packet till the packet is declared as failed.
        /// </summary>
        protected TimeSpan _timeout = TimeSpan.FromSeconds(1);

        public Queue<string> Errors { get; protected set; } = new Queue<string>();

        /// <summary>
        /// Is the PacketSenderReceiver currently connected?
        /// </summary>
        public bool IsConnected => this.State switch
        {
            States.Connecting => false,
            States.NotStarted => false,
            States.Idle => true,
            States.WaitingForAnswer => true,
            _ => throw new ArgumentOutOfRangeException($"{nameof(this.State)}: {this.State.ToString()}")
        };

        /// <summary>
        /// Gets the current state of the PacketSenderReceiver.
        /// </summary>
        public States State { get; protected set; } = States.NotStarted;

        /// <summary>
        /// Connects the PacketSenderReceiver to the device.
        /// </summary>
        public async Task<bool> Connect()
        {
            this.State = States.Connecting;
            if (await this.ConnectInternal())
            {
                this.State = States.Idle;
                return true;
            }
            else
            {
                this.State = States.NotStarted;
                return false;
            }
        }

        /// <summary>
        /// Send a packet to the device.
        /// </summary>
        public async Task<PacketSendResult> SendPacket(PayloadTypes  payloadType, string payload) 
        {
            if (this.State == States.NotStarted)
            {
                Error("can't send packet when not started!");
                return new PacketSendResult
                {
                    OriginalPacketId = null,
                    Ok = false,
                    Message = "can't send packet when not started!",
                };
            }

            if (this.State == States.Connecting || this.State == States.WaitingForAnswer)
            {
                var waitTill = DateTime.UtcNow + _timeout / 2;
                while (State != States.Idle && DateTime.UtcNow < waitTill)
                    await Task.Delay(1);

                if (State != States.Idle)
                {
                    var errMsgNotReady = $"can't send packet on com port while connecting or waiting for answer! State is {State.ToString()}";
                    Error(errMsgNotReady);
                    return new PacketSendResult
                    {
                        OriginalPacketId = null,
                        Ok = false,
                        Message = errMsgNotReady,
                    };
                }
            }

            if (State == States.Idle)
            {
                this.State = States.WaitingForAnswer;
                var answer = await this.SendPacketInternal(payloadType, payload);
                this.State = States.Idle;
                if (answer == null)
                    return new PacketSendResult { Ok = false, Message = "Packet answer == null!" };

                return answer;
            }
            else
            {
                var errMsg = $"Unhandled {nameof(State)}:{State.ToString()}";
                Error(errMsg);
                return new PacketSendResult
                {
                    Ok = false,
                    Message = errMsg
                };
            }
        }

        public abstract void Dispose();

        protected abstract Task<bool> ConnectInternal();
        protected abstract Task<PacketSendResult> SendPacketInternal(PayloadTypes payloadType, string payload);

        protected void Error(string message)
        {
            this.Errors.Enqueue(message);
            this.ErrorOccured?.Invoke(this, new ErrorEventArgs(message));
        }
    }
}
