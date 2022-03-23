using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class ScreenFade : MonoBehaviour
{
    // References
    public ForwardRendererData rendererData = null;

    // Settings
    [Range(0, 1)]
    public float alpha = 1.0f;
    [Range(0, 5)]
    public float duration = 0.5f;

    // Runtime
    public Renderer fadePlane = null;

    public Material fadeMaterial;

    private void Start()
    {
        // Find the, and set the feature's material
        SetupFadeFeature();

        //fadeMaterial = fadePlane.material;
    }

    private void SetupFadeFeature()
    {
        // Look for the screen fade feature
        ScriptableRendererFeature feature = rendererData.rendererFeatures.Find(item => item is ScreenFadeFeature);

        // Ensure it's the correct feature
        if (feature is ScreenFadeFeature screenFade)
        {
            // Duplicate material so we don't change the renderer's asset
            fadeMaterial = Instantiate(screenFade.settings.material);
            screenFade.settings.runTimeMaterial = fadeMaterial;
        }
    }

    public float FadeIn()
    {
        // Fade to black
        Debug.Log("Fade In");
        StartCoroutine(Fade(0, 1));
        return duration;
    }


    public float FadeOut()
    {
        // Fade to clear
        Debug.Log("Fade Out");
        StartCoroutine(Fade(1, 0));
        return duration;
    }

    private IEnumerator Fade(float startValue, float endValue)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime <= duration)
        {
            // figure out blend value
            float blend = elapsedTime / duration;
            elapsedTime += Time.deltaTime;

            // apply intensity
            float intensity = Mathf.Lerp(startValue, endValue, blend);
            ApplyValue(intensity);
        }
        yield return null;
    }

    private void ApplyValue(float value)
    {
        // override original intensity
        fadeMaterial.SetFloat("unity_Lightmaps", value);
    }
}
