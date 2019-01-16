using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageManager : MonoBehaviour
{
    public NamedImage[] images;
    public Sprite defaultImage;

    public Sprite FindImage(string name)
    {
        foreach(NamedImage image in images)
        {
            if (image.name == name)
                return image.image;
        }

        return defaultImage;
    }
}

[System.Serializable]
public struct NamedImage
{
    public string name;
    public Sprite image;
}
