// Communicate between different devices on dotnet or arduino via COM port or Wifi
// https://github.com/Springwald/PacketLogistics
//
// (C) 2023 Daniel Springwald, Bochum Germany
// Springwald Software  -   www.springwald.de
// daniel@springwald.de -  +49 234 298 788 46
// All rights reserved
// Licensed under MIT License

using PacketLogistics.ComPorts.ComportPackets;

namespace PacketLogistics
{
    public abstract class PacketSenderReceiver: IDisposable
    {
        public class PacketReceivedEventArgs : EventArgs
        {
            public byte[] Payload { get; }

            public PacketReceivedEventArgs(byte[] payload)
            {
                Payload = payload;
            }
        }

        public class ErrorEventArgs : EventArgs
        {
            public string Message { get; }

            public ErrorEventArgs(String message)
            {
                Message = message;
            }
        }

        public event EventHandler<PacketReceivedEventArgs>? PacketReceived;
        public event EventHandler<ErrorEventArgs>? ErrorOccured;


        public enum States
        {
            NotStarted,
            Connecting,
            Idle,
            WaitingForAnswer
        }


        protected TimeSpan _timeout = TimeSpan.FromSeconds(1);

        public Queue<string> Errors { get; protected set; } = new Queue<string>();

        public bool IsConnected => this.State switch
        {
            States.Connecting => false,
            States.NotStarted => false,
            States.Idle => true,
            States.WaitingForAnswer => true,
            _ => throw new ArgumentOutOfRangeException($"{nameof(this.State)}: {this.State.ToString()}")
        };

        public States State { get; protected set; } = States.NotStarted;

        protected void Error(string message)
        {
            this.Errors.Enqueue(message);
            this.ErrorOccured?.Invoke(this, new ErrorEventArgs(message));
        }

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

        public async Task<PacketSendResult> SendPacket(byte[] payload)
        {
            if (this.State == States.NotStarted)
            {
                Error("can't send packet on not started com port!");
                return new PacketSendResult
                {
                    OriginalPacketTimestampUtc = DateTime.UtcNow,
                    OriginalPacketId = null,
                    Ok = false,
                    Message = "can't send packet on not started com port!",
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
                        OriginalPacketTimestampUtc = DateTime.UtcNow,
                        OriginalPacketId = null,
                        Ok = false,
                        Message = errMsgNotReady,
                    };
                }
            }

            if (State == States.Idle)
            {
                this.State = States.WaitingForAnswer;

                var answer = await this.SendPacketInternal(payload);

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

        protected abstract Task<bool> ConnectInternal();
        protected abstract Task<PacketSendResult> SendPacketInternal(byte[] payload);
        public abstract void Dispose();
    }
}
