using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// TODO: Create a class hierarchy to customize build behavior for various targets if
// it becomes necessary to target more than Windows on the x86_64 architecture.
/**
 * This is a Unity script that contains a set of utility functions that can be used in and out
 * of the Unity Editor to build a project.
 */
public class BuildUtils : MonoBehaviour
{
    static string GetBuildName()
    {
        var buildNumber = System.Environment.GetEnvironmentVariable("BUILD_BUILDNUMBER"); // Team Foundation Server specific
        if (buildNumber == null)
        {
            buildNumber = "Dev";
        }

        var changeSet = System.Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION"); // Team Foundation Server specific
        if (changeSet == null)
        {
            changeSet = "0";
        }

        return buildNumber + "." + changeSet;
    }

    static string GetLongBuildName(BuildTarget target, string buildName)
    {
        return Application.productName + "_" + target.ToString() + "_" + buildName;
    }

    /* This method creates the path where the build will end up, dependent on 
     * what build target is and the what the name of the build is.
     * 
     */
    private static string GetBuildPath(BuildTarget target, string buildName)
    {
        var buildOutputPath = System.Environment.GetEnvironmentVariable("BUILD_BINARIESDIRECTORY");
        if (buildOutputPath == null)
        {
            buildOutputPath = "";
        }
        else
        {
            buildOutputPath += "/";
        }

        return buildOutputPath + "Builds/" + target.ToString() + "/" + GetLongBuildName(target, buildName);
    }

    private static void BuildAssetBundles(string buildPath, BuildTarget target, bool v1, bool v2, bool v3)
    {
        throw new NotImplementedException();
    }

    private static UnityEditor.Build.Reporting.BuildReport BuildGame(string buildPath, string executableName, BuildTarget target, BuildOptions opts, string buildId, bool il2cpp)
    {
        var levels = UnityEditor.EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
        var exePathName = buildPath + "/" + executableName;

        Debug.Log("Building: " + exePathName);
        Directory.CreateDirectory(buildPath);

        // Set all files to be writeable (As Unity 2017.1 sets them to read only)

        string fullBuildPath = Directory.GetCurrentDirectory() + "/" + buildPath;

        string[] fileNames = Directory.GetFiles(fullBuildPath, "*.*", SearchOption.AllDirectories);

        //Contentpipeline compile player scripts

        foreach (var fileName in fileNames)
        {
            FileAttributes attributes = File.GetAttributes(fileName);
            attributes &= ~FileAttributes.ReadOnly;
            File.SetAttributes(fileName, attributes);
        }


        var monoDirs = Directory.GetDirectories(fullBuildPath).Where(s => s.Contains("MonoBleedingEdge"));
        var il2cppDirs = Directory.GetDirectories(fullBuildPath).Where(s => s.Contains("BackUpThisFolder_ButDontShipItWithYourGame"));
        var clearFolder = (il2cpp && monoDirs.Count() > 0) || (!il2cpp && il2cppDirs.Count() > 0);
        if (clearFolder)
        {
            Debug.Log(" Deleting old folders ..");
            foreach (var file in Directory.GetFiles(fullBuildPath))
                File.Delete(file);

            foreach (var dir in monoDirs)
                Directory.Delete(dir, true);

            foreach (var dir in il2cppDirs)
                Directory.Delete(dir, true);

            foreach (var dir in Directory.GetDirectories(fullBuildPath).Where(s => s.EndsWith("_Data")))
                Directory.Delete(dir, true);
        }

        if (il2cpp)
        {
            UnityEditor.PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
            UnityEditor.PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Standalone, Il2CppCompilerConfiguration.Release);
        }
        else
        {
            UnityEditor.PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        }

        var prevBuildIdEnvVar = Environment.GetEnvironmentVariable("BUILD_ID", EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("BUILD_ID", buildId, EnvironmentVariableTarget.Process);
        var result = BuildPipeline.BuildPlayer(levels, exePathName, target, opts);
        Environment.SetEnvironmentVariable("BUILD_ID", prevBuildIdEnvVar, EnvironmentVariableTarget.Process);

        Debug.Log(" ==== Build Done ====");

        var stepCount = result.steps.Count();
        Debug.Log(" Steps:" + stepCount);
        for (var i = 0; i < stepCount; i++)
        {
            var step = result.steps[i];
            Debug.Log("-- " + (i + 1) + "/" + stepCount + " " + step.name + " " + step.duration.Seconds + "s --");
            foreach (var msg in step.messages)
                Debug.Log(msg.content);
        }

        return result;
    }

    /* This method creates a build for a 64-bit Windows environment.
     *
     * NOTE: This method by default builds the project using the Mono scripting backend.
     *
     */
    [MenuItem("BuildUtils/BuildSystem/Win64/CreateBuildWindows64")]
    public static void CreateBuildWin64()
    {
        Debug.Log("Win64 build started.");
        var target = BuildTarget.StandaloneWindows64;
        var buildName = GetBuildName();
        var buildPath = GetBuildPath(target, buildName);
        string executableName = Application.productName + ".exe";

        Directory.CreateDirectory(buildPath);

        /* TODO: Finish implementing this so that it does the following (similar to FPSSample project from Unity):
         * 
         * 1. Build assets uncompressed
         * 2. Determine if we need to force a bundle rebuild
         * 3. Go through "levels" and create asset bundles for each level
         * 4. Go through separate assets and bundle those
         */
        //BuildAssetBundles(buildPath, target, true, true, true);

        var result = BuildGame(buildPath, executableName, target, BuildOptions.None, buildName, false);

        if (!result)
            throw new Exception("BuildPipeline.BuildPlayer failed");
        if (result.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            throw new Exception("BuildPipeline.BuildPlayer failed: " + result.ToString());

        Debug.Log("Win64 build completed...");
    }
}
