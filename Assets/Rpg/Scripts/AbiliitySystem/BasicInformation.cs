using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicInformation {
    private string name;
    private string description;
    private Sprite icon;

    public BasicInformation(string name)
    {
        this.name = name;
    }

    public BasicInformation(string name, string description)
    {
        this.name = name;
        this.description = description;
    }

    public BasicInformation(string name, string description, Sprite icon)
    {
        this.name = name;
        this.description = description;
        this.icon = icon;
    }
    public string ObjectName
    {
        get { return name; }
    }
    public string ObjectDescription
    {
        get { return description; }
    }
    public Sprite ObjectIcon
    {
        get { return icon; }
    }
}
