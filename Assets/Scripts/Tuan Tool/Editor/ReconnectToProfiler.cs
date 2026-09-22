using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TuanTool
{
    public class ReconnectToProfiler : MonoBehaviour
    {
        [MenuItem("Tools/" + "Reconnect Profiler")]
        public static void ReconnectProfiler()
        {
            Debug.Log($"bunder ver :{Application.identifier}");
            runCMD($"/C adb forward tcp:34999 localabstract:Unity-{Application.identifier}");
            runCMD($"/C adb reverse tcp:34998 tcp:34999");

            static void runCMD(string cmd)
            {
                System.Diagnostics.Process process = new System.Diagnostics.Process();
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = cmd;
                process.StartInfo = startInfo;
                process.Start();
            }
        }
    }
}
