using UnityEngine;

public class AssignThemeLoader : MonoBehaviour
{
    void Start()
    {
        // Start the recursive check from the current GameObject
        CheckAndAssignThemeLoader(transform);
    }

    // Recursive method to check and assign ThemeLoader
    private void CheckAndAssignThemeLoader(Transform parent)
    {
        // Iterate through all child objects of the current parent
        foreach (Transform child in parent)
        {
            // Check if the child has the tag "Enemy"
            if (child.CompareTag("Enemy"))
            {
                // Check if the ThemeLoader component already exists
                if (child.GetComponent<ThemeLoader>() == null)
                {
                    // Add the ThemeLoader script to the child
                    child.gameObject.AddComponent<ThemeManager>();
                    child.gameObject.GetComponent<ThemeManager>().LoadTheme();
                }
            }

            // Recursively call this method for the child object
            CheckAndAssignThemeLoader(child);
        }
    }
}
