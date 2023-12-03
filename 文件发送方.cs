var udpClient = new UdpClient("127.0.0.1", 11000);
var fileBytes = File.ReadAllBytes("YourFilePathHere");
int packetSize = 1020, startByte = 0; // 设置开始字节位置

udpClient.Send(BitConverter.GetBytes(0).Concat(BitConverter.GetBytes(startByte / packetSize)).ToArray(), 8);
for (int i = startByte; i < fileBytes.Length; i += packetSize) {
    var packet = BitConverter.GetBytes(i / packetSize).Concat(fileBytes.Skip(i).Take(packetSize)).ToArray();
    for (int attempt = 0, sent = 0; attempt < 3 && sent == 0; attempt++) {
        try { udpClient.Send(packet, packet.Length); sent = 1; } catch { Thread.Sleep(10); }
    }
}
udpClient.Send(BitConverter.GetBytes(-1).Concat(BitConverter.GetBytes(fileBytes.Length)).ToArray(), 8);
udpClient.Close();