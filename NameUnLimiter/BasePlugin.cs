using BepInEx;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("denyscrasav4ik.basicallyukrainian.nameunlimiter", "No Name Restrictions", "1.1.0")]
public class NoName : BaseUnityPlugin
{
    private void Awake()
    {
        var harmony = new Harmony("denyscrasav4ik.basicallyukrainian.nameunlimiter");
        harmony.PatchAll();
        Logger.LogInfo("No Name Restrictions loaded!");
    }
}

[HarmonyPatch(typeof(NameManager), "AddLetterToName")]
public class Patch_AddLetterToName
{
    static bool Prefix(NameManager __instance, ref string addition)
    {
        if (string.IsNullOrEmpty(addition))
            return false;

        char[] forbidden = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        foreach (char f in forbidden)
            addition = addition.Replace(f.ToString(), "");

        if (string.IsNullOrEmpty(addition))
            return false;

        var newNameField = __instance
            .GetType()
            .GetField(
                "newName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
        string newName = (string)newNameField.GetValue(__instance);
        newName += addition;
        newNameField.SetValue(__instance, newName);

        var uncappedField = __instance
            .GetType()
            .GetField(
                "uncapped",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );
        bool uncapped = (bool)uncappedField.GetValue(__instance);
        if (!uncapped)
        {
            uncappedField.SetValue(__instance, true);
            __instance.capitalLetters.SetActive(false);
            __instance.lowercaseLetters.SetActive(true);
        }

        return false;
    }
}

[HarmonyPatch(typeof(NameManager), "Update")]
public class Patch_NameManager_Update
{
    static bool Prefix(NameManager __instance)
    {
        bool enteringNewName = (bool)
            __instance
                .GetType()
                .GetField(
                    "enteringNewName",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .GetValue(__instance);
        bool errorOpen = (bool)
            __instance
                .GetType()
                .GetField(
                    "errorOpen",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .GetValue(__instance);
        bool loadingFile = (bool)
            __instance
                .GetType()
                .GetField(
                    "loadingFile",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )
                .GetValue(__instance);
        var newNameField = __instance
            .GetType()
            .GetField(
                "newName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

        if (!enteringNewName || errorOpen || loadingFile)
            return true;

        string newName = (string)newNameField.GetValue(__instance);

        if (Input.GetKeyDown(KeyCode.Backspace) && newName.Length > 0)
        {
            newName = newName.Remove(newName.Length - 1, 1);
            newNameField.SetValue(__instance, newName);
        }

        string input = Input.inputString;
        if (!string.IsNullOrEmpty(input))
        {
            char[] forbidden = { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
            foreach (char f in forbidden)
                input = input.Replace(f.ToString(), "");
            input = input.Replace("\b", "").Replace("\n", "").Replace("\r", "");

            if (!string.IsNullOrEmpty(input))
            {
                newName += input;
                newNameField.SetValue(__instance, newName);

                var uncappedField = __instance
                    .GetType()
                    .GetField(
                        "uncapped",
                        System.Reflection.BindingFlags.NonPublic
                            | System.Reflection.BindingFlags.Instance
                    );
                bool uncapped = (bool)uncappedField.GetValue(__instance);
                if (!uncapped)
                {
                    uncappedField.SetValue(__instance, true);
                    __instance.capitalLetters.SetActive(false);
                    __instance.lowercaseLetters.SetActive(true);
                }
            }
        }
        if (__instance.newNameTmp != null)
        {
            __instance.newNameTmp.enableAutoSizing = true;
        }
        __instance.newNameTmp.text = newName;

        bool hasName = newName.Length > 0;
        __instance.nameInstructions.SetActive(!hasName);
        __instance.submitActive.SetActive(hasName);
        __instance.submitDisabled.SetActive(!hasName);

        if (Input.GetKeyDown(KeyCode.Return))
        {
            __instance
                .GetType()
                .GetMethod("NameClicked")
                .Invoke(__instance, new object[] { __instance.nameCount });
        }

        return false;
    }

    [HarmonyPatch(typeof(NameButton), "UpdateState")]
    public class Patch_NameButton_UpdateState
    {
        static void Postfix(NameButton __instance)
        {
            if (__instance.text != null)
            {
                __instance.text.enableAutoSizing = true;

                __instance.text.fontSizeMax = 24;
            }
        }
    }
}
