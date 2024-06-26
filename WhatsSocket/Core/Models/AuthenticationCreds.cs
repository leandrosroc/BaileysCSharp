﻿using Newtonsoft.Json;
using Proto;
using System.Diagnostics.CodeAnalysis;

namespace WhatsSocket.Core.Models
{
    //public class ProcessedHistoryMessageKey
    //{
    //    [JsonProperty("remoteJid")]
    //    public string RemoteJid { get; set; }

    //    [JsonProperty("fromMe")]
    //    public bool FromMe { get; set; }

    //    [JsonProperty("id")]
    //    public string Id { get; set; }
    //}

    public class ProcessedHistoryMessage
    {
        [JsonProperty("key")]
        public MessageKey Key { get; set; }

        [JsonProperty("messageTimestamp")]
        public ulong MessageTimestamp { get; set; }
    }

    public class Account
    {
        [JsonProperty("details")]
        public byte[] Details { get; set; }

        [JsonProperty("accountSignatureKey")]
        public byte[] AccountSignatureKey { get; set; }

        [JsonProperty("accountSignature")]
        public byte[] AccountSignature { get; set; }

        [JsonProperty("deviceSignature")]
        public byte[] DeviceSignature { get; set; }
    }

    public partial class AuthenticationCreds
    {
        public AuthenticationCreds()
        {
            ProcessedHistoryMessages = new List<ProcessedHistoryMessage>();
        }

        [JsonProperty("me")]
        public ContactModel Me { get; set; }

        [JsonProperty("noiseKey")]
        public KeyPair NoiseKey { get; set; }

        [JsonProperty("pairingEphemeralKeyPair")]
        public KeyPair PairingEphemeralKeyPair { get; set; }

        [JsonProperty("signedIdentityKey")]
        public KeyPair SignedIdentityKey { get; set; }

        [JsonProperty("signedPreKey")]
        public SignedPreKey SignedPreKey { get; set; }

        [JsonProperty("registrationId")]
        public int RegistrationId { get; set; }

        [JsonProperty("advSecretKey")]
        public string AdvSecretKey { get; set; }

        [JsonProperty("processedHistoryMessages")]
        public List<ProcessedHistoryMessage> ProcessedHistoryMessages { get; set; }

        [JsonProperty("nextPreKeyId")]
        public uint NextPreKeyId { get; set; }

        [JsonProperty("firstUnuploadedPreKeyId")]
        public uint FirstUnuploadedPreKeyId { get; set; }

        [JsonProperty("accountSyncCounter")]
        public int AccountSyncCounter { get; set; }

        [JsonProperty("accountSettings")]
        public AccountSettings AccountSettings { get; set; }

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("phoneId")]
        public string PhoneId { get; set; }

        [JsonProperty("identityId")]
        public byte[] IdentityId { get; set; }

        [JsonProperty("registered")]
        public bool Registered { get; set; }

        [JsonProperty("backupToken")]
        public byte[] BackupToken { get; set; }

        [JsonProperty("registration")]
        public Registration Registration { get; set; }

        [JsonProperty("account")]
        public Account Account { get; set; }

        [JsonProperty("signalIdentities")]
        public SignalIdentity[] SignalIdentities { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; }


        [JsonProperty("myAppStateKeyId")]
        public string MyAppStateKeyId { get; set; }

        public static string Serialize(AuthenticationCreds? creds)
        {
            return JsonConvert.SerializeObject(creds, Formatting.Indented, new BufferConverter());
        }


        public static AuthenticationCreds? Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<AuthenticationCreds>(json, new BufferConverter());
        }

        public ulong? LastAccountTypeSync { get; set; }



    }

    internal class BufferConverter : JsonConverter<byte[]>
    {
        public override byte[]? ReadJson(JsonReader reader, Type objectType, byte[]? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonException();
            }

            //Read first property
            reader.Read();
            if (reader.TokenType == JsonToken.PropertyName)
            {
                //Make sure it has a property 'type'
                if (reader.Value?.ToString() == "type")
                {
                    var value = reader.ReadAsString();
                    if (value == "Buffer")
                    {
                        //Read the data value
                        reader.Read();
                        value = reader.ReadAsString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            existingValue = Convert.FromBase64String(value);
                        }
                    }
                    else
                    {
                        throw new JsonException("the type is not defined as a Buffer");
                    }
                }

            }
            reader.Read();


            return existingValue;
        }

        public override void WriteJson(JsonWriter writer, byte[]? value, JsonSerializer serializer)
        {
            var data = JsonConvert.SerializeObject(new
            {
                type = "Buffer",
                data = Convert.ToBase64String(value)
            });

            //writer.WriteStartObject();

            writer.WriteRawValue(data);

            //writer.WriteEndObject();
        }
    }


}
