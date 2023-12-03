using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

var listener = new UdpClient(11000);
var groupEP = new IPEndPoint(IPAddress.Any, 11000);
var receivedData = new ConcurrentDictionary<int, byte[]>();
bool endSignalReceived = false;
int fileSize = 0, startPacket = 0;
var allDone = new ManualResetEvent(false);

new Thread(() => {
    while (!endSignalReceived) {
        var bytes = listener.Receive(ref groupEP);
        int packetNum = BitConverter.ToInt32(bytes, 0);
        if (bytes.Length == 5) { fileSize = packetNum == -1 ? BitConverter.ToInt32(bytes, 1) : 0; endSignalReceived = true; }
        else if (packetNum == 0) startPacket = BitConverter.ToInt32(bytes, 1);
        else receivedData.TryAdd(packetNum, bytes[4..]);
    }
    allDone.Set();
}).Start();

allDone.WaitOne();
var fileContent = new byte[fileSize - startPacket * 1020];
foreach (var kvp in receivedData.OrderBy(kvp => kvp.Key)) Array.Copy(kvp.Value, 0, fileContent, (kvp.Key - startPacket) * 1020, kvp.Value.Length);
File.WriteAllBytes("ReceivedFile", fileContent);
Console.WriteLine($"MD5 Hash: {BitConverter.ToString(MD5.Create().ComputeHash(fileContent)).Replace("-", "").ToLower()}");
listener.Close();