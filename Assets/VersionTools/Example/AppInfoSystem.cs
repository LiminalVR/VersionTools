using System.Collections;
using System.Collections.Generic;
using BuildTools;
using UnityEngine;
using UnityEngine.UI;

public class AppInfoSystem : MonoBehaviour
{
    public VersionSettings VersionSettings;

    public Text VersionLabel;

    private void Start()
    {
        VersionLabel.text = VersionSettings.Version.ToString();
    }
}
