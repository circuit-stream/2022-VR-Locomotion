using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;

public class VignetteApplier : MonoBehaviour
{
    public float intensity = 0.75f;
    public float duration = 0.5f;
    public Volume volume = null;

    public LocomotionProvider locomotionProvider;
    private Vignette vignette;
    
    void Awake()
    {
        // get the provider
        //locomotionProvider = GetComponent<LocomotionProvider>();

        // get the vignette
        if(volume.profile.TryGet(out Vignette vignette))
        {
            this.vignette = vignette;
        }
    }
    private void OnEnable()
    {
        // subscribe 
        locomotionProvider.beginLocomotion += FadeIn;
        locomotionProvider.endLocomotion += FadeOut;

    }
    private void OnDisable()
    {
        // unsubscribe
        locomotionProvider.beginLocomotion -= FadeIn;
        locomotionProvider.endLocomotion -= FadeOut;
    }


    public void FadeIn(LocomotionSystem locomotionSystem)
    {
        // tween to intensity
        Debug.Log("Fade In");
        StartCoroutine(Fade(0, intensity));
    }

    public void FadeOut(LocomotionSystem locomotionSystem)
    {
        // tween to zero
        Debug.Log("Fade Out");
        StartCoroutine(Fade(intensity, 0));
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


        yield return new WaitForEndOfFrame();
    }

    private void ApplyValue(float value)
    {
        // override original intensity
        vignette.intensity.Override(value);
    }
}
