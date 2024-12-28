using UnityEngine;

public class ThemeLoader : MonoBehaviour
{
    void ThemeLoaded()
    {
        // Get the folder path from PlayerPrefs
        string folderPath = PlayerPrefs.GetString("Theme", "Jungle");

        // Get the current sprite's name from the SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer component found on this GameObject.");
            return;
        }

        string spriteName = spriteRenderer.sprite.name;

        // Load the sprite from the Resources folder
        string resourcePath = folderPath + "/" + spriteName;
        Sprite loadedSprite = Resources.Load<Sprite>(resourcePath);

        // Check if the sprite was found and assign it
        if (loadedSprite != null)
        {
            spriteRenderer.sprite = loadedSprite;
        }
        else
        {
            Debug.LogError($"Sprite not found at path: Resources/{resourcePath}");
        }
    }
}
