﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cowboy.Codec.Mqtt
{
    public sealed class CONNECT : ControlPacket
    {
        public static readonly IDictionary<string, byte> VersionLevels = new Dictionary<string, byte>()
        {
            { "3.1.1", 0x04 },
        };

        public CONNECT()
        {
            this.ProtocolName = "MQTT";
            this.ProtocolVersion = "3.1.1";
            this.KeepAliveInterval = TimeSpan.FromSeconds(30);
            this.CleanSession = true;
        }

        public CONNECT(string clientIdentifier)
            : this()
        {
            this.ClientIdentifier = clientIdentifier;
        }

        public override ControlPacketType ControlPacketType { get { return ControlPacketType.CONNECT; } }

        protected override List<byte> GetVariableHeaderBytes()
        {
            var variableHeaderBytes = new List<byte>();

            if (string.IsNullOrWhiteSpace(this.ProtocolName))
                throw new InvalidControlPacketException(
                    string.Format("Invalid protocol name [{0}].", this.ProtocolName));
            if (string.IsNullOrWhiteSpace(this.ProtocolVersion))
                throw new InvalidControlPacketException(
                    string.Format("Invalid protocol version [{0}].", this.ProtocolVersion));
            if (!VersionLevels.ContainsKey(this.ProtocolVersion))
                throw new InvalidControlPacketException(
                    string.Format("Cannot support version [{0} {1}].", this.ProtocolName, this.ProtocolVersion));


            variableHeaderBytes.AddRange(MqttEncoding.Default.GetBytes(this.ProtocolName));

            var protocolLevel = VersionLevels[this.ProtocolVersion];
            variableHeaderBytes.Add(protocolLevel);

            byte connectFlags = 0x0;

            if (this.UserNameFlag)
                connectFlags = (byte)(connectFlags | 0x80);
            if (this.PasswordFlag)
                connectFlags = (byte)(connectFlags | 0x40);
            if (this.WillRetain)
                connectFlags = (byte)(connectFlags | 0x20);
            if (this.WillFlag)
                connectFlags = (byte)(connectFlags | 0x04);
            if (this.CleanSession)
                connectFlags = (byte)(connectFlags | 0x02);

            switch (this.WillQos)
            {
                case WillQosLevel.QoS0:
                    break;
                case WillQosLevel.QoS1:
                    connectFlags = (byte)(connectFlags | 0x08);
                    break;
                case WillQosLevel.QoS2:
                    connectFlags = (byte)(connectFlags | 0x10);
                    break;
                default:
                    break;
            }

            variableHeaderBytes.Add(connectFlags);

            short keepAliveSeconds = (short)this.KeepAliveInterval.TotalSeconds;
            variableHeaderBytes.Add((byte)(keepAliveSeconds >> 8));
            variableHeaderBytes.Add((byte)(keepAliveSeconds & 0xFF));

            return variableHeaderBytes;
        }

        protected override List<byte> GetPayloadBytes()
        {
            var payload = new List<byte>();

            if (string.IsNullOrWhiteSpace(this.ClientIdentifier))
                throw new InvalidControlPacketException(
                    string.Format("Invalid client identifier [{0}].", this.ClientIdentifier));

            payload.AddRange(MqttEncoding.Default.GetBytes(ClientIdentifier));

            if (this.WillFlag)
            {
                payload.AddRange(MqttEncoding.Default.GetBytes(WillTopic));
                payload.AddRange(MqttEncoding.Default.GetBytes(WillMessage));
            }

            if (this.UserNameFlag)
            {
                payload.AddRange(MqttEncoding.Default.GetBytes(UserName));
            }

            if (this.PasswordFlag)
            {
                payload.AddRange(MqttEncoding.Default.GetBytes(Password));
            }

            return payload;
        }

        public string ClientIdentifier { get; set; }
        public string WillTopic { get; set; }
        public string WillMessage { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string ProtocolName { get; set; }
        public string ProtocolVersion { get; set; }
        public TimeSpan KeepAliveInterval { get; set; }

        public bool UserNameFlag { get; set; }
        public bool PasswordFlag { get; set; }
        public bool WillRetain { get; set; }
        public WillQosLevel WillQos { get; set; }
        public bool WillFlag { get; set; }
        public bool CleanSession { get; set; }

        public enum WillQosLevel
        {
            QoS0 = 0,
            QoS1 = 1,
            QoS2 = 2,
        }
    }
}