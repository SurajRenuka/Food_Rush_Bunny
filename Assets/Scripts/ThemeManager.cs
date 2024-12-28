using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    void OnEnable()
    {
        EnsureDefaultMaterial();
        LoadTheme();
    }

    private void EnsureDefaultMaterial()
    {
        // Check if any parent has "Type" included in its name
        Transform currentTransform = transform;
        while (currentTransform != null)
        {
            if (currentTransform.name.Contains("Type"))
            {
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    // Assign Unity's built-in Sprites-Default material directly
                    spriteRenderer.sharedMaterial = new Material(Shader.Find("Sprites/Default"));
                }
                return; // Exit once the condition is met
            }
            currentTransform = currentTransform.parent; // Move to the parent transform
        }
    }


    public void LoadTheme()
    {
        EnsureDefaultMaterial();
        // Get the folder path from PlayerPrefs
        string folderPath = PlayerPrefs.GetString("Theme", "Jungle"); // Default fallback to "DefaultTheme" if not set
        // Get the current sprite's name from the SpriteRenderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
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
