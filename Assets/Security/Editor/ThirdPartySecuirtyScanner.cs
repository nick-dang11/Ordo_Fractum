
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ThirdPartySecurityScanner
{
    private const string TargetFolder =
        "Assets/StarterAssets/ThirdPersonController/Scripts";

    private static readonly string[] SensitiveAPIs =
    {
        "UnityWebRequest",
        "HttpClient",
        "WebRequest",
        "File.ReadAllText",
        "File.ReadAllBytes",
        "File.Open",
        "Directory.GetFiles",
        "Process.Start",
        "Application.persistentDataPath"
    };

    [MenuItem("Tools/Security/Scan Third-Party Scripts")]
    public static void ScanScripts()
    {
        if (!Directory.Exists(TargetFolder))
        {
            Debug.LogError(
                "[Security Scan] Target folder not found: " +
                TargetFolder);
            return;
        }

        string[] files = Directory.GetFiles(
            TargetFolder, "*.cs", SearchOption.AllDirectories);

        int totalFindings = 0;

        Debug.Log("[Security Scan] Starting scan...");
        Debug.Log("[Security Scan] Scripts found: " + files.Length);

        foreach (string file in files)
        {
            string[] lines;

            try
            {
                lines = File.ReadAllLines(file);
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[Security Scan] Could not read " +
                    file + ": " + ex.Message);
                continue;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                foreach (string api in SensitiveAPIs)
                {
                    if (lines[i].Contains(api))
                    {
                        totalFindings++;

                        Debug.LogWarning(
                            "[Security Scan] REVIEW: " +
                            file +
                            " | Line " + (i + 1) +
                            " | API: " + api);
                    }
                }
            }
        }

        Debug.Log(
            "[Security Scan] Completed. Files scanned: " +
            files.Length +
            " | Potential API matches: " + totalFindings);

        Debug.Log(
            "[Security Scan] Matches require manual review. " +
            "No matches does not prove a script is secure.");
    }
}
