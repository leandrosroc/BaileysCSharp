﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhatsSocket.Core.Curve;
using WhatsSocket.Core.Helper;
using WhatsSocket.Core.Models;
using WhatsSocket.Core;
using System.Xml.Linq;
using Newtonsoft.Json;
using WhatsSocket.Core.Models.Sessions;
using WhatsSocket.Core.WABinary;
using WhatsSocket.Core.Utils;
using Org.BouncyCastle.Bcpg;
using Proto;

namespace WhatsSocketConsole
{
    public static class Tests
    {

        public static void RunTests()
        {
            BufferSendTest();
            TestSendMessageBuffer();
            GenerateMessages();
            TestInflate();
            LtHashTest();
            TestSliceMinusEnd();
            TestExpandedHKDF();
            TestPingEncoding();
            TestEncodeAndDecode();
            TestDecodeFrame();
            TestHKDIF();
            TestEncoder();
            TestDecodeQRNode();
            TestVerifyCurve();
            TestSign();
            TestSuccessSign();
            TestDeriveSecret();

        }

        private static void BufferSendTest()
        {
            var node = BufferReader.DecodeDecompressedBinaryNode(Convert.FromBase64String("APgICwj7CLrlDYF95ZgXBCYO+v+GJ3l3mBefA/gC+AIb+AX4BA4P+v+GJ2ZSRQZ/A/gB+AYWNjMEQvzDMwohBdo4LxbdraQw90Gtl1x3U2WdKELGN34lewqkJcTvc7RHEAcYACKQAW2ZVE1xAbOxbkQLjwWZqyUGpT30TRvIfJrpsHWT6kcmh6wRruK5F7Fx62n23BYm4UANtPQloPAqbDaeq+ht8QgvObGjApI8Mo+B/H3k7OC7AqsMPg2AIj8vGuITnpEC9b7HG+7qMWh/MQk8ME0c6IzEADiQfLtOhfsSsON51/IhiKF5+dH6E2yU1tSIE0fG9uKZmxj7pBj6+AQOD/cAUP+GJ2ZSRQZ/+AH4BhY2MwRC/MMzCiEFj+ahruhNXaDXWM4PU+omH2EPRw1c8qDTVS5h3Tt1E0sQAhgDIpAB3Q2HdAJnERYJt3xIzlOLgGj19WX71rD9CiIlyvrmfKVBeXepziYfGEIQaapZvErdxMfilA531w2YadMxTw6igDydFzk2TMkDQ3JygeJpgRGwuXmOyV9vNxj16f537CoI7+YseoQ6FR+MgyeGOB9yS83hS1ODp6BzcnUYvmuomhP2Nwk66cMoXhn+1pqeRZQ94RkS6rF6cnv4BA4P+v+GJ3l3mBefA/gB+AYWNjMEQvySMwohBfW88FuhkQ8QK72mFqAkUsJWdDAmbg4o/+YDa9BAdxMxEAAYACJgUZZApwqmpMIcEfoxsapQUla6StmPYRcOGDEcrhJ4QVCkSDcayWzhbfaqpBnhRjR/XFSO0lRTH1pa1/n11p1uzkJOYG9+UI+DT1fdfTS4wbLY/8m6ZNXc6RGgEmV0KEReWPxRyL6rk2/4BA4P9wAR/4YneXeYF5/4AfgGFjYzBGD85TMI6M/0BxIhBd86eAcY95BIrbq+k3Pjh9fttRpR9KbEikq+xAwsKZkXGiEFmWdGujQtBWMOWKLlxnzKePOHxREOJa9QkXR2OPVJsW8ikgEzCiEF+lTOIzUad+kjIeWU9QHrTbSmJkLahocaM7Fp1h2pNkUQBxgAImBv8WSH87dTY3Rg1KQJAPPD8X0zgdgSPieq5JDzWYkL01rvbqRoCCMtJihxQ68XJa+kS97gZO/Fttwd9WyzNqjB1TLEM1zCN1snCFkXbhQR1QgWdZm5IA2Ci9salYyZ4fjnMJSKxpBjFSgfMAH4BA4P9wAS/4YneXeYF5/4AfgGFjYzBGD84zMIlQISIQW7sfTeJ0xY6Qp+gnIz0txpNxvNurtNyow0iOypCMnEGxohBZlnRro0LQVjDlii5cZ8ynjzh8URDiWvUJF0djj1SbFvIpIBMwohBQscUnirhQi4CBCNiuPEEtAOorer5ikJlczWWravVcsgEAcYACJgrspHZm0FUo7Bb1z5Y47QRhbtG9FPfs3TvU84SI5nZH7WS6i3trWNJr5T7jnqM9dC3a3EGoe7+1Wtj9Idb+NcPA04PUoQKEhsu4FneDs+EkGBCEpaUL8m6QLVvPNy0YQoG4fZPCkoM1YoHzAB+AL8D2RldmljZS1pZGVudGl0efy6ChIIrLXFiAYQwLqesAYYBCAAKAASIIEzLR+KRLAOmi2o/Jy6LSI8i3TJ4kBJxmVUqmvP645FGkAq9jQQiYS1kgkQJpxY5rA4l094DXskfN8JP/XH74nQkY+39kSisGaXt2BxmUZQVIIjqmEmF29fwK+75dLT/ZwBIkD50exmM1MdcJ0c1kseorh5HZmdFbkBZIOLjXZMtW+ZvPdBcf58D0UttbNAJl0FnrgqZsaHdcvE0O/KbvvIGPaP"));

            var json = JsonConvert.SerializeObject(node, Formatting.Indented);

        }

        private static void TestSendMessageBuffer()
        {
            var json = "{\"tag\":\"iq\",\"attrs\":{\"to\":\"@s.whatsapp.net\",\"type\":\"get\",\"xmlns\":\"usync\",\"id\":\"46858.8451-10\"},\"content\":[{\"tag\":\"usync\",\"attrs\":{\"sid\":\"46858.8451-9\",\"mode\":\"query\",\"last\":\"true\",\"index\":\"0\",\"context\":\"message\"},\"content\":[{\"tag\":\"query\",\"attrs\":{},\"content\":[{\"tag\":\"devices\",\"attrs\":{\"version\":\"2\"}}]},{\"tag\":\"list\",\"attrs\":{},\"content\":[{\"tag\":\"user\",\"attrs\":{\"jid\":\"27665245067@s.whatsapp.net\"}},{\"tag\":\"user\",\"attrs\":{\"jid\":\"27797798179@s.whatsapp.net\"}}]}]}]}";

            var binaryNode = JsonConvert.DeserializeObject<BinaryNode>(json);

            var endode = BufferWriter.EncodeBinaryNode(binaryNode);
            var base64 = endode.ToByteArray().ToBase64();

            Debug.Assert(base64 == "APgKHg76AAMEMRmyCP+HRoWLhFGhD/gB+Ayy/ANzaWT/BkaFi4RRqZGgWs/RT7cL+AL4AqD4AfgDKE0z+AJu+AL4AxAP+v+GJ2ZSRQZ/A/gDEA/6/4YneXeYF58D");



            var iq = new BinaryNode()
            {
                tag = "iq",
                attrs =
                {
                    {"to", "@s.whatsapp.net" },
                    {"type" , "get" },
                    {"xmlns","usync" },
                    {"id","7834.23612-8" }
                },
                content = new BinaryNode[]
    {
                    new BinaryNode()
                    {
                        tag = "usync",
                        attrs =
                        {
                            {"sid", "7834.23612-7" },
                            {"mode","usync" },
                            {"last","true" },
                            {"index","0" },
                            {"context","message" }
                        },
                        content = new BinaryNode[]
                        {
                            new BinaryNode()
                            {
                                tag = "query",
                                content = new BinaryNode[]
                                {
                                    new BinaryNode()
                                    {
                                        tag = "devices",
                                        attrs =
                                        {
                                            {"version", "2" }
                                        }
                                    }
                                },
                            },
                            new BinaryNode()
                            {
                                tag = "list",
                                content = new BinaryNode[]
                                {
                                    new BinaryNode()
                                    {
                                        tag = "user",
                                        attrs =
                                        {
                                            {"jid","27665245067@s.whatsapp.net" }
                                        }
                                    },
                                    new BinaryNode()
                                    {
                                        tag = "user",
                                        attrs =
                                        {
                                            {"jid","27797798179@s.whatsapp.net" }
                                        }
                                    }
                                }
                            }
                        }
                    }
    }
            };
            endode = BufferWriter.EncodeBinaryNode(iq);
            var buffer = endode.ToByteArray();
        }

        private static void GenerateMessages()
        {
            var reply = @"{
  ""key"": {
    ""remoteJid"": ""27797798179@s.whatsapp.net"",
    ""fromMe"": false,
    ""id"": ""FBDBCD0EC4256FA59F2BF8C3B9439002"",
    ""participant"": """"
  },
  ""message"": {
    ""conversation"": ""Hello"",
    ""messageContextInfo"": {
      ""deviceListMetadata"": {
        ""senderKeyHash"": ""qerq+rAf42paLA=="",
        ""senderTimestamp"": ""1711460193"",
        ""recipientKeyHash"": ""H/ksOwnzLPeHdg=="",
        ""recipientTimestamp"": ""1711546000""
      },
      ""deviceListMetadataVersion"": 2
    }
  },
  ""status"": ""SERVER_ACK"",
  ""broadcast"": false,
  ""pushName"": ""Donald Jansen""
}";

            var replyTo = WebMessageInfo.Parser.ParseJson(reply);
            var options = new MessageGenerationOptionsFromContent()
            {
                Quoted = replyTo
            };

            var message = MessageUtil.GenerateWAMessageContent(new ExtendedTextMessageModel() { Text = "oh hello there" }, options);
            var wamessage = MessageUtil.GenerateWAMessageFromContent(replyTo.Key.RemoteJid, message, options);
        }

        private static void TestInflate()
        {
            var buffer = Convert.FromBase64String("eAHjYLHS5pIyMje3NDe3tDA0t3Qo1ivPSCwpTiwo0MtLLRHidcnPS8xJUfBKzCtOzQMAE8oNnQ==");

            var inflat = BufferReader.Inflate(buffer);

            Debug.Assert(Convert.ToBase64String(inflat) == "CAQ6KwoaMjc3OTc3OTgxNzlAcy53aGF0c2FwcC5uZXQSDURvbmFsZCBKYW5zZW4=");
        }

        private static void LtHashTest()
        {
            var ss = Convert.FromBase64String("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=");

            var addBuffs = new List<byte[]>
            {
                Convert.FromBase64String("tpAlo9TRdclDonMD7ragdYFjKP5VaXuXokzOtxtM2u8="),
            };
            var subBuffs = new List<byte[]>();

            var ah = new HashAntiTampering("WhatsApp Patch Integrity");

            var result = ah.SubstarctThenAdd(ss, addBuffs, subBuffs);

        }

        private static void TestSliceMinusEnd()
        {
            var buffer = Convert.FromBase64String("Q9QximJn3r0imIXilMioUt6XLzDAGe4HTQup189OgKvsfu7M5N/g/bHN/5L3m3vGOgqPwaYfGsb4i43bSYo3MzKFKZgkqX0LCISFjQtxK64yo/M3VbO2OJoTfZLMIq89");
            var a = buffer.Slice(0, -32).ToBase64();
            Debug.Assert(a == "Q9QximJn3r0imIXilMioUt6XLzDAGe4HTQup189OgKvsfu7M5N/g/bHN/5L3m3vGOgqPwaYfGsb4i43bSYo3Mw==");
            var b = buffer.Slice(-32).ToBase64();
            Debug.Assert(b == "MoUpmCSpfQsIhIWNC3ErrjKj8zdVs7Y4mhN9kswirz0=");
        }

        private static void TestExpandedHKDF()
        {
            var result = WhatsSocket.Core.Helper.CryptoUtils.HKDF(Convert.FromBase64String("IpLK/bLtOJpZa7mw+9/Cn6v7EF2YfxvyrSs1K+tvFoE="), 112, [], Encoding.UTF8.GetBytes("WhatsApp App State Keys"));


            //var IV = result.Slice(0, 16);
            //var CipherKey = result.Slice(16, 48);
            //var MacKey = result.Slice(48, 80);
        }

        private static void TestDeriveSecret()
        {
            var a = Convert.FromBase64String("OBRpl4+nI8GjhHO2AJ7PTnk0kNjlbwckoXExgitm6dQ=");
            var result = WhatsSocket.Core.Helper.CryptoUtils.DeriveSecrets(a, new byte[32], Encoding.UTF8.GetBytes("WhisperMessageKeys"));
        }

        private static void TestSuccessSign()
        {
            var @private = "KOwFT3vxL5IcJp+wKvIv2gmeHwlQY2V0ZkkmApQZ5GI=";
            var deviceMsg = "BgEI/f7nsAIQoP+3rQYYKCAAKACrRV5Gg5U97GP9ty08k+6gNVYvFnSceP9fsqNiYoGMbLy6K/AKoi08AC8i2wd0pHCLQ6zZSn/PJ6oTDx7DvLBi";
            var deviceSignature = "zqAkA3s+PMr9YV+nKGT8gOojEH4P/Cp0VruJwBJlrH2JJ/nrSmqQ7zhSt1q0qcBvDMqXVItiSyDT3eKBelzkhQ==";

            var newSig = Curve25519.Sign(Convert.FromBase64String(@private), Convert.FromBase64String(deviceMsg));
            var base64 = Convert.ToBase64String(newSig);
            Debug.Assert(deviceSignature == base64);


            var reply = "APgIHg76AAMEFAj/hTCIKJVv+AH4AvwQcGFpci1kZXZpY2Utc2lnbvgB+AT8D2RldmljZS1pZGVudGl0efwJa2V5LWluZGV4/wFB/JgKEgj9/uewAhD4h7itBhgpIAAoABpApzrqAU8tQpwaIFVQeg5azu2ZEbAtsNiJlJzEi7MDq1A61gTkVWy6MRrKbiiw6JCG1F0t/PP6BI6kM8hqzQD5BSJAtHkqkFvLxlN78cHNCqZ8UYHC9ZS4xol5fyU5srBbmaPxHULG/MDtyRueqZYel6VN1uCf19HZvkYER2TUlPxUgw==";
            var data = BufferReader.DecodeDecompressedBinaryNode(Convert.FromBase64String(reply));


            var encoded = BufferWriter.EncodeBinaryNode(data).ToByteArray();
            base64 = Convert.ToBase64String(encoded);

            Debug.Assert(base64 == reply);
        }

        private static void TestVerifyCurve()
        {

            var deviceDetails = "CP3+57ACELqtra0GGB4gACgA";
            var accountSignatureKey = "vLor8AqiLTwALyLbB3SkcItDrNlKf88nqhMPHsO8sGI=";
            var signedIdentityKeypublic = "EJHw6gOAb0JnlmiC+UzEIlZEeNFBwb6Tjukb3qjPZS4=";
            var accountSignature = "rXS7h0kpENziw/rStkiNf3De3lI8/pNhnztmC3hcVBDr4SgjvrPzfxHhcVsqVe1TJ4QqzdpMtOzD6Og8xGoBCQ==";


            /*  
             *  Type script matching code
             *  var deviceDetails =  Buffer.from("CP3+57ACELqtra0GGB4gACgA", 'base64');
             *  var signedIdentityKeypublic =  Buffer.from("EJHw6gOAb0JnlmiC+UzEIlZEeNFBwb6Tjukb3qjPZS4=", 'base64');
             *  var accountSignatureKey =  Buffer.from("vLor8AqiLTwALyLbB3SkcItDrNlKf88nqhMPHsO8sGI=", 'base64');
             *  var accountSignature =  Buffer.from("rXS7h0kpENziw/rStkiNf3De3lI8/pNhnztmC3hcVBDr4SgjvrPzfxHhcVsqVe1TJ4QqzdpMtOzD6Og8xGoBCQ==", 'base64');
             *  const accountMsg = Buffer.concat([ Buffer.from([6, 0]), deviceDetails, signedIdentityKeypublic ])
             *  var valid= Curve.verify(accountSignatureKey, accountMsg, accountSignature);
            */


            var accountMsg = new byte[] { 6, 0 }
            .Concat(Convert.FromBase64String(deviceDetails))
            .Concat(Convert.FromBase64String(signedIdentityKeypublic))
            .ToArray();

            var valid = WhatsSocket.Core.Helper.CryptoUtils.Verify(Convert.FromBase64String(accountSignatureKey), accountMsg, Convert.FromBase64String(accountSignature));
        }

        private static void TestSign()
        {
            /*  
             *  Type script matching code
             *  var a =  Buffer.from("IDCDtVD++rJolLsVvMBmamuh55HDuoLumCOurRPk9G4=", 'base64');
	         *  var b =  Buffer.from("Bd9bnwurvCcsEHAQl6EX3NnKBezsyX+ecp1vuGHCxmoC", 'base64');
	         *  var c = Curve.sign(a,b);//c = m5xXDNaJF/QmcrrQblztk/0QNmVwZket5PNSbdI+CGD7rC9KfIGKPwa3gI9ZW6kAXViZto9/4faaTm0rKsUwgg==
	         *  console.log(c.toString('base64')) 
            */


            var result = Curve25519.Sign(Convert.FromBase64String("IDCDtVD++rJolLsVvMBmamuh55HDuoLumCOurRPk9G4="), Convert.FromBase64String("Bd9bnwurvCcsEHAQl6EX3NnKBezsyX+ecp1vuGHCxmoC"));
            var signB64 = Convert.ToBase64String(result);
            Debug.Assert(signB64 == "m5xXDNaJF/QmcrrQblztk/0QNmVwZket5PNSbdI+CGD7rC9KfIGKPwa3gI9ZW6kAXViZto9/4faaTm0rKsUwgg==");
        }

        private static void TestDecodeQRNode()
        {
            var qrNode = Convert.FromBase64String("APgKHgb6AAMEPgj7BTU3lSSUGfwCbWT4AfgC/AtwYWlyLWRldmljZfgG+AL8A3JlZvxSMkBSa0ZwQVJsZmdBazNRQXlzSjA3RVVBWThNUEdrZWtveE4zamlMK0swL0VaK0pXbEhIbE50TTk3ZUNBcm5ScXNQNFplRStUMXRzRks5YXc9PfgC/ANyZWb8UjJATGVVckdJMmlidTl0S01TTXlsR0ZIcWtsRG42aC96UGxlK2xmOTY2czNmWlZEc1hueUFqQ29EczZEdUkyMUF3WWVMWlBTSUpKTkJEVzVBPT34AvwDcmVm/FIyQFZ3RVQ2TWdXQ1I4L3J5MW1IWTI2elFNb2I5cVl4czBEbE53OFErRmhYblhDQlN0Wk5TSEh4a0pJaTBuZ2hzU1VDdUt6Rmtxamg3UkEyUT09+AL8A3JlZvxSMkBlYmNHOHlIenZsOEhNYUFrMjRZWkJwWkFpdk1MNkZLWkc3Qnp3WG5lejM2OXgzT292dlVRbzRiRWdtbTNsd3NOU3ZWZFpucEFVVWVuM1E9PfgC/ANyZWb8UjJAVkRUOEhBeWl3aGIxc1N1K2xaaWtQVmlhY0oyL3JXNGFZeGF2THNBaTRsa1FUelJwYSsxNEZJOHRtUFNoUDBVaU5JbEhBNnN0S0FramVBPT34AvwDcmVm/FIyQFBnY21selB2S1hWM2hZbUJtQnBDRjJpckUyNEc2TmFzNGVIL1lxQURZK01oZFFweTlxSHJTbmtIZmFsZWFJNGp5SnA0em84ZW10ejkrUT09");
            var decoded = BufferReader.DecodeDecompressedBinaryNode(qrNode);


            var encoding = ((decoded.content as BinaryNode[])?.FirstOrDefault()?.content as BinaryNode[])?.FirstOrDefault()?.content as byte[];

            var @ref2 = Encoding.UTF8.GetString(encoding ?? new byte[] { 0 });
            var @ref = "2@RkFpARlfgAk3QAysJ07EUAY8MPGkekoxN3jiL+K0/EZ+JWlHHlNtM97eCArnRqsP4ZeE+T1tsFK9aw==";
            Debug.Assert(@ref == @ref2);
        }

        private static void TestPingEncoding()
        {
            var iq = new BinaryNode()
            {
                tag = "iq",
                attrs = new Dictionary<string, string>()
                    {
                        {"id", "23788.8381-1" },
                        {"to",Constants.S_WHATSAPP_NET },
                        {"type","get" },
                        {"xmlns" ,"w:p" }
                    },
                content = new BinaryNode[]
                    {
                        new BinaryNode()
                        {
                            tag = "ping",

                        }
                    }
            };

            var data = BufferWriter.EncodeBinaryNode(iq).ToByteArray();
            var data2 = Convert.FromBase64String("APgKHgj/BiN4i4OBoQ76AAMEMRlf+AH4AVA=");
            var iq2 = BufferReader.DecodeDecompressedBinaryNode(data2);
            Debug.Assert(iq.attrs["id"] == iq2.attrs["id"]);
        }

        private static void TestEncodeAndDecode()
        {
            var iq = new BinaryNode()
            {
                tag = "iq",
                attrs = new Dictionary<string, string>()
                {
                    {"to", Constants.S_WHATSAPP_NET },
                    {"type","result" },
                    {"id", "1763566167" }
                }
            };

            //Check Ecoding
            var data = BufferWriter.EncodeBinaryNode(iq).ToByteArray();
            var data2 = Encoding.UTF8.GetString(data);
            var iq2 = BufferReader.DecodeDecompressedBinaryNode(data);


            Debug.Assert(iq.attrs["id"] == iq2.attrs["id"]);
        }

        private static void TestDecodeFrame()
        {
            var decrypted = Convert.FromBase64String("APgKHgb6AAMEPgj7hSYngHkPGfwCbWT4AfgC/AtwYWlyLWRldmljZfgG+AL8A3JlZvxSMkBaMklsOWdJaDhFUWxKTDJlY3FjZjVjNG1YSE5ZZlMrdWIwYTlpV0ZIay9lSHg1TkhrQ2dDZk50c0hIUmt6S3VqeUdoSmc5WUswOTdBWkE9PfgC/ANyZWb8UjJAT0VXdENLNElJdUZrdytvUGhkNHdmRWgxQ21CWlZCMEZZVGcxZkdadGpXWmlBS29EWXk1NXBuZ0FPb2QvMTRESDJRSzdraE54cGZ6aHpBPT34AvwDcmVm/FIyQHF6a3RmdDRBTW0vY1Q2ZUtyUllhNWZmK2lEVnl0UmdHSVRPSHQzOG53RmtIdEpLV0crTlgzT0wwYjFBUGdwNFE5QkVZclJlZ0hnQiszUT09+AL8A3JlZvxSMkBsVEdwMExZQ3UzazMvL1lqRkVZMmVjKytHaG1YcDNHN3c4aExEMVR5ZmdKMTZrWGM2bGlkd0FlNjczb0dhaDFYV2RFV1VQZWZWRWNGMXc9PfgC/ANyZWb8UjJAaUx1WTBEdDB3QU10QWJLNEhURG85L0NuUHBUQWQ0eHh2MThqWWxpOXFadlFsSTd2NjdUMlUwbjhPeHU0NGFCbXNuUGJJWnZrbmY4WWNnPT34AvwDcmVm/FIyQFFCN1gycm1ocnNvdHNZb0Z4emRVeHhJQmFjazhZajYxQzM0RHlUQktUWXNlSU5LeW1ockdYUWRkaWlZVCtSN3JvRzV3eHgzR3phS0l0UT09");
            var node = BufferReader.DecodeDecompressedBinaryNode(decrypted);
            Debug.Assert(node.attrs["id"] == "262780790");
        }


        private static void TestEncoder()
        {
            //Test From First Send Node
            //Result should be if id is 
            var iq = new BinaryNode()
            {
                tag = "iq",
                attrs = new Dictionary<string, string>()
                {
                    {"to", Constants.S_WHATSAPP_NET },
                    {"type","result" },
                    {"id", "1763566167" }
                }
            };

            //Check Ecoding
            var data = BufferWriter.EncodeBinaryNode(iq).ToByteArray();
            var base64 = Convert.ToBase64String(data);
            Debug.Assert(base64 == "APgHHg76AAMEFAj/BRdjVmFn");



            //Check EncodeFrame
            var noise = new NoiseHandler(new KeyPair() { Public = new byte[0], Private = new byte[0] }, new Logger());
            noise.EncKey = Convert.FromBase64String("IQ0axqM7VdppHVYjjgv+f1hv0ioMHb8zWPJqRBTCB+I=");
            noise.IsFinished = true;
            noise.SetIntro = true;
            noise.Hash = new byte[0];
            var resultToSend = noise.EncodeFrame(data);
            var resultb64 = Convert.ToBase64String(resultToSend);
            Debug.Assert(resultb64 == "AAAiqGhn9eons9qb858cYl4gHUlphcTYWJdLhZ5DCNIRHB+z/Q==");


        }

        private static void TestHKDIF()
        {
            /* RESULT FROM NODE PROJECT ON HKDIF
            HKDIF 1
{
  kdf: 'frvaFLM4ZMjur+3gOkWkOJh3Phpb1Q2voTS2tVkDCzU=',
  salt: 'Tm9pc2VfWFhfMjU1MTlfQUVTR0NNX1NIQTI1NgAAAAA=',
  key: 'nc0uKa/NKY545AdJxcom2iBZy6xAUJqt+UgJDAn/KxTiM0+nIw7VqbX5Y4dvcq9eA3o6TjdZRtFxUY0gBTSBAw==',
  s1: 'nc0uKa/NKY545AdJxcom2iBZy6xAUJqt+UgJDAn/KxQ=',
  s2: '4jNPpyMO1am1+WOHb3KvXgN6Ok43WUbRcVGNIAU0gQM='
}
            */
            var hkdf = WhatsSocket.Core.Helper.CryptoUtils.HKDF(Convert.FromBase64String("frvaFLM4ZMjur+3gOkWkOJh3Phpb1Q2voTS2tVkDCzU="), 64, Convert.FromBase64String("Tm9pc2VfWFhfMjU1MTlfQUVTR0NNX1NIQTI1NgAAAAA="), Encoding.UTF8.GetBytes(""));
            var b64 = Convert.ToBase64String(hkdf);

            var split1 = Convert.ToBase64String(hkdf.Take(32).ToArray());
            var split2 = Convert.ToBase64String(hkdf.Skip(32).ToArray());
            Debug.Assert(split1 == "nc0uKa/NKY545AdJxcom2iBZy6xAUJqt+UgJDAn/KxQ=");
            Debug.Assert(split2 == "4jNPpyMO1am1+WOHb3KvXgN6Ok43WUbRcVGNIAU0gQM=");
        }
    }
}
