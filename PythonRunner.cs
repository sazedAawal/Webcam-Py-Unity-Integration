using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class PythonRunner : MonoBehaviour
{
    [Header("Python Configuration")]
    [Tooltip("The path to your Python executable. Example: C:\\Python311\\python.exe or just 'python' if in system PATH")]
    public string pythonExecutable = "python";

    [Tooltip("The absolute file path to your working Python script.")]
    public string pythonScriptPath = "C:\\Path\\To\\Your\\Script.py";

    private Process pythonProcess;

    void Start()
    {
        RunPythonScript();
    }

    void RunPythonScript()
    {
        try
        {
            // Verify file exists before trying to run it
            if (!File.Exists(pythonScriptPath))
            {
                UnityEngine.Debug.LogError($"[PythonRunner] Script not found at: {pythonScriptPath}");
                return;
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = pythonExecutable;
            
            // Pass the script path as an argument to the python executable
            startInfo.Arguments = $"\"{pythonScriptPath}\""; 
            
            // Configuration to make it run seamlessly
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = false; // Set to true later if you want to hide the cmd window
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;

            // Start the background process
            pythonProcess = new Process();
            pythonProcess.StartInfo = startInfo;
            
            // Hook up Unity's console to read Python's print statements
            pythonProcess.OutputDataReceived += (sender, args) => { if (args.Data != null) UnityEngine.Debug.Log($"[Python Out]: {args.Data}"); };
            pythonProcess.ErrorDataReceived += (sender, args) => { if (args.Data != null) UnityEngine.Debug.LogError($"[Python Err]: {args.Data}"); };

            pythonProcess.Start();
            pythonProcess.BeginOutputReadLine();
            pythonProcess.BeginErrorReadLine();

            UnityEngine.Debug.Log("[PythonRunner] Python script launched successfully!");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"[PythonRunner] Failed to start Python process: {e.Message}");
        }
    }

    // Crucial: This kills the Python script automatically when you exit Play Mode in Unity
    void OnDestroy()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
            pythonProcess.Dispose();
            UnityEngine.Debug.Log("[PythonRunner] Python process cleanly terminated.");
        }
    }
}