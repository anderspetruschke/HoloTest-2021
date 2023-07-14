using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloTray
{
  private static string m_address;
  private static int m_port = 12058;

  [Serializable]
  public class Response
  {
    public Response(int a_code, string a_message)
    {
      code = a_code;
      message = a_message;
    }

    public int code;
    public string message;
  }

  public static void SetServer(string address) { m_address = address; }

  public static bool LaunchHoloView(int port)
  {
    Response response = SendRecv(@"{
      ""command"": ""launch"",
      ""args"": {
        ""app"": ""holoview"",
        ""closeOthers"": true,
        ""cmdln"": [ " + port.ToString() + @"]
        }
      }"
    );

    if (response.code != 200)
      Debug.Log("Failed to launch Holo View Server (code: " + response.code + ", msg: " + response.message + ")");
    return response.code == 200;
  }

  public static bool KillHoloView()
  {
    Response response = SendRecv(@"{
      ""command"": ""kill"",
      ""args"": {
        ""app"": ""holoview"",
        ""force"": true
        }
      }"
    );

    if (response.code != 200)
      Debug.Log("Failed to close Holo View Server (code: " + response.code + ", msg: " + response.message + ")");
    return response.code == 200;
  }

  public static Response SendRecv(string message)
  {
    try
    {
      TcpClient client = new TcpClient(m_address, m_port);

      // Send the message
      StreamWriter writer = new StreamWriter(client.GetStream());
      client.Client.Send(Encoding.ASCII.GetBytes(message));
      client.Client.Send(new byte[1] { 0 });

      // Read the response
      int readSize = 255;
      byte[] readBuffer = new byte[readSize];
      string recvMessage = "";
      try
      {
        while (client.Client.Receive(readBuffer, SocketFlags.Peek) != 0)
        {
          int numBytes = client.Client.Receive(readBuffer);

          // If the last byte read == 0, the end of the message has been recieved
          bool isEnd = numBytes > 0 && readBuffer[numBytes - 1] == 0;

          // Convert the bytes to a string and append it to the read message
          recvMessage += Encoding.ASCII.GetString(readBuffer, 0, numBytes);
          if (isEnd)
            break;
        }
      }
      catch (Exception) { }

      return JsonUtility.FromJson<Response>(recvMessage);
    }
    catch (Exception ex)
    {
      return new Response(-1, ex.Message);
    }
  }
}
