//Do not edit! This file was generated by Unity-ROS MessageGeneration.
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Unity.Robotics.ROSTCPConnector.MessageGeneration;

namespace RosMessageTypes.ZenithInterfaces
{
    [Serializable]
    public class ChangeStateRequest : Message
    {
        public const string k_RosMessageName = "zenith_interfaces/ChangeState";
        public override string RosMessageName => k_RosMessageName;

        public string requested_state;

        public ChangeStateRequest()
        {
            this.requested_state = "";
        }

        public ChangeStateRequest(string requested_state)
        {
            this.requested_state = requested_state;
        }

        public static ChangeStateRequest Deserialize(MessageDeserializer deserializer) => new ChangeStateRequest(deserializer);

        private ChangeStateRequest(MessageDeserializer deserializer)
        {
            deserializer.Read(out this.requested_state);
        }

        public override void SerializeTo(MessageSerializer serializer)
        {
            serializer.Write(this.requested_state);
        }

        public override string ToString()
        {
            return "ChangeStateRequest: " +
            "\nrequested_state: " + requested_state.ToString();
        }

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [UnityEngine.RuntimeInitializeOnLoadMethod]
#endif
        public static void Register()
        {
            MessageRegistry.Register(k_RosMessageName, Deserialize);
        }
    }
}
