using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System;
public class AndroidUpdate : MonoBehaviour {

    void Start() {
        StartCoroutine(downLoadFromServer());
    }

    IEnumerator downLoadFromServer() {
        string url = "http://www.688pk.com/online.apk";


        string savePath = Path.Combine(Application.persistentDataPath, "data");
        savePath = Path.Combine(savePath, "AntiOvr.apk");

        Dictionary<string, string> header = new Dictionary<string, string>();
        string userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
        header.Add("User-Agent", userAgent);
        WWW www = new WWW(url, null, header);


        while (!www.isDone) {
            //Must yield below/wait for a frame
            GameObject.Find("TextDebug").GetComponent<Text>().text = "Stat: " + www.progress;
            yield return null;
        }

        byte[] yourBytes = www.bytes;

        GameObject.Find("TextDebug").GetComponent<Text>().text = "Done downloading. Size: " + yourBytes.Length;


        //Create Directory if it does not exist
        if (!Directory.Exists(Path.GetDirectoryName(savePath))) {
            Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            GameObject.Find("TextDebug").GetComponent<Text>().text = "Created Dir";
        }

        try {
            //Now Save it
            System.IO.File.WriteAllBytes(savePath, yourBytes);
            Debug.Log("Saved Data to: " + savePath.Replace("/", "\\"));
            GameObject.Find("TextDebug").GetComponent<Text>().text = "Saved Data";
        } catch (Exception e) {
            Debug.LogWarning("Failed To Save Data to: " + savePath.Replace("/", "\\"));
            Debug.LogWarning("Error: " + e.Message);
            GameObject.Find("TextDebug").GetComponent<Text>().text = "Error Saving Data";
        }

        //Install APK
        installAppAbove24(savePath);
    }

    public bool installApp(string apkPath) {
        try {
            AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
            string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
            int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);

            AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            AndroidJavaClass uriObj = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject uri = uriObj.CallStatic<AndroidJavaObject>("fromFile", fileObj);

            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("setClassName", "com.android.packageinstaller", "com.android.packageinstaller.PackageInstallerActivity");

            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            currentActivity.Call("startActivity", intent);

            GameObject.Find("TextDebug").GetComponent<Text>().text = "Success";
            return true;
        } catch (System.Exception e) {
            GameObject.Find("TextDebug").GetComponent<Text>().text = "Error: " + e.Message;
            return false;
        }
    }

    private bool installAppAbove24(string apkPath) {
        bool success = true;
        GameObject.Find("TextDebug").GetComponent<Text>().text = "Installing App";

        try {
            //Get Activity then Context
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject unityContext = currentActivity.Call<AndroidJavaObject>("getApplicationContext");

            //Get the package Name
            string packageName = unityContext.Call<string>("getPackageName");
            string authority = packageName + ".fileprovider";

            AndroidJavaClass intentObj = new AndroidJavaClass("android.content.Intent");
            string ACTION_VIEW = intentObj.GetStatic<string>("ACTION_VIEW");
            AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", ACTION_VIEW);


            int FLAG_ACTIVITY_NEW_TASK = intentObj.GetStatic<int>("FLAG_ACTIVITY_NEW_TASK");
            int FLAG_GRANT_READ_URI_PERMISSION = intentObj.GetStatic<int>("FLAG_GRANT_READ_URI_PERMISSION");

            //File fileObj = new File(String pathname);
            AndroidJavaObject fileObj = new AndroidJavaObject("java.io.File", apkPath);
            //FileProvider object that will be used to call it static function
            AndroidJavaClass fileProvider = new AndroidJavaClass("android.support.v4.content.FileProvider");
            //getUriForFile(Context context, String authority, File file)
            AndroidJavaObject uri = fileProvider.CallStatic<AndroidJavaObject>("getUriForFile", unityContext, authority, fileObj);

            intent.Call<AndroidJavaObject>("setDataAndType", uri, "application/vnd.android.package-archive");
            intent.Call<AndroidJavaObject>("addFlags", FLAG_ACTIVITY_NEW_TASK);
            intent.Call<AndroidJavaObject>("addFlags", FLAG_GRANT_READ_URI_PERMISSION);
            currentActivity.Call("startActivity", intent);

            GameObject.Find("TextDebug").GetComponent<Text>().text = "Success";
        } catch (System.Exception e) {
            GameObject.Find("TextDebug").GetComponent<Text>().text = "Error: " + e.Message;
            success = false;
        }

        return success;
    }

}
