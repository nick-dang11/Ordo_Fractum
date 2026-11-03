
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ThirdPartySecurityScanner
{
    // Original third-party scripts
    private const string TargetFolder =
        "Assets/StarterAssets/ThirdPersonController/Scripts";

    // Separate folder for security test scripts
    private const string TestFolder =
        "Assets/Security/Tests";

    // Potentially sensitive API patterns
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

    // Scan original third-party scripts
    [MenuItem("Tools/Security/Scan Third-Party Scripts")]
    public static void ScanScripts()
    {
        ScanFolder(TargetFolder);
    }

    // Scan security test scripts
    [MenuItem("Tools/Security/Scan Test Scripts")]
    public static void ScanTestScripts()
    {
        ScanFolder(TestFolder);
    }

    // Reusable scanning method
    private static void ScanFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Debug.LogError(
                "[Security Scan] Folder not found: " + folder);
            return;
        }

        string[] files = Directory.GetFiles(
            folder, "*.cs", SearchOption.AllDirectories);

        int totalFindings = 0;
        int filesRead = 0;
        int filesFailed = 0;

        Debug.Log(
            "[Security Scan] Starting scan: " + folder);

        Debug.Log(
            "[Security Scan] Scripts found: " + files.Length);

        foreach (string file in files)
        {
            string[] lines;

            try
            {
                lines = File.ReadAllLines(file);
                filesRead++;
            }
            catch (Exception ex)
            {
                filesFailed++;

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
            "[Security Scan] Completed. Files found: " +
            files.Length +
            " | Files read: " + filesRead +
            " | Read errors: " + filesFailed +
            " | Potential API matches: " + totalFindings);

        Debug.Log(
            "[Security Scan] Matches require manual review. " +
            "No matches does not prove a script is secure.");
    }
}
