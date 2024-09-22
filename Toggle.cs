using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class ObjectToggle : MonoBehaviour
{
    public string targetTag = "HandTag";
    public List<GameObject> objectsToToggle = new List<GameObject>();

    public bool enableFading = true;
    public float fadeDuration = 1.0f;

    public bool enableSaving = true;
    private string saveKey = "SavedObjectStates";

    public bool enableMaterialChange = false;
    public bool enablePressedMaterialChange = false;
    public Material pressedMaterial;
    public float pressedMaterialDuration = 1.0f;

    public bool enableDisabling = false;

    
    public bool enableMultiToggle = false;
    public List<GameObject> objectsToEnable = new List<GameObject>(); 
    public List<GameObject> objectsToDisable = new List<GameObject>(); 

    private Dictionary<GameObject, Material[]> originalMaterials = new Dictionary<GameObject, Material[]>();

    private void Start()
    {
        if (enableSaving)
        {
            LoadSavedStates();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            ToggleObjects();
        }
    }

    public void ToggleObjects()
    {
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
            {
                if (enableFading)
                {
                    StartCoroutine(FadeObject(obj, !obj.activeSelf, fadeDuration));
                }
                else
                {
                    obj.SetActive(!obj.activeSelf);
                }

                if (enableMaterialChange)
                {
                    ChangeMaterial(obj);
                }

                if (enablePressedMaterialChange && pressedMaterial != null)
                {
                    StartCoroutine(ChangeToPressedMaterial(obj));
                }
            }
        }

        if (enableDisabling)
        {
            EnableDisableObjects();
        }

        if (enableMultiToggle)
        {
            MultiToggleObjects();
        }

        if (enableSaving)
        {
            SaveStates();
        }
    }

    private void ChangeMaterial(GameObject obj)
    {
        if (!originalMaterials.ContainsKey(obj))
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            Material[] currentMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                currentMaterials[i] = renderers[i].material;
            }
            originalMaterials[obj] = currentMaterials;
        }
    }

    private IEnumerator ChangeToPressedMaterial(GameObject obj)
    {
        if (!originalMaterials.ContainsKey(obj))
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            Material[] currentMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                currentMaterials[i] = renderers[i].material;
            }
            originalMaterials[obj] = currentMaterials;
        }

        Renderer[] objRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in objRenderers)
        {
            renderer.material = pressedMaterial;
        }

        yield return new WaitForSeconds(pressedMaterialDuration);

        for (int i = 0; i < objRenderers.Length; i++)
        {
            objRenderers[i].material = originalMaterials[obj][i];
        }
    }

    private void EnableDisableObjects()
    {
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

    
    private void MultiToggleObjects()
    {
        if (objectsToEnable.Count > 0 || objectsToDisable.Count > 0)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }

            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }

    private IEnumerator FadeObject(GameObject obj, bool fadeIn, float duration)
    {
        if (fadeIn) obj.SetActive(true);
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material material in renderer.materials)
                {
                    if (material.HasProperty("_Color"))
                    {
                        Color color = material.color;
                        color.a = newAlpha;
                        material.color = color;
                    }
                }
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.HasProperty("_Color"))
                {
                    Color color = material.color;
                    color.a = endAlpha;
                    material.color = color;
                }
            }
        }

        if (!fadeIn) obj.SetActive(false);
    }

    private void SaveStates()
    {
        List<int> objectStates = new List<int>();
        foreach (GameObject obj in objectsToToggle)
        {
            objectStates.Add(obj.activeSelf ? 1 : 0);
        }
        PlayerPrefs.SetString(saveKey, string.Join(",", objectStates));
        PlayerPrefs.Save();
    }

    private void LoadSavedStates()
    {
        if (PlayerPrefs.HasKey(saveKey))
        {
            string savedData = PlayerPrefs.GetString(saveKey);
            string[] savedStates = savedData.Split(',');

            for (int i = 0; i < objectsToToggle.Count && i < savedStates.Length; i++)
            {
                bool isActive = savedStates[i] == "1";
                objectsToToggle[i].SetActive(isActive);
            }
        }
    }
}

[CustomEditor(typeof(ObjectToggle))]
public class ObjectToggleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ObjectToggle myScript = (ObjectToggle)target;
        if (GUILayout.Button("Test Toggle"))
        {
            myScript.ToggleObjects();
        }
    }
}
