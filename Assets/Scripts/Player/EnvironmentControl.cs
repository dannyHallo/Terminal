using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentControl : MonoBehaviour
{
    public AtmosphereSettings atmosphereSettings;

    public float timeOfDay;
    float sunDistance;
    float sunSpeed;
    bool allowTimeFlow;
    public GameObject sun;

    private void Update()
    {
        if (!sun)
            sun = transform.Find("Sun").gameObject;

        if (!Application.isPlaying)
            timeOfDay = atmosphereSettings.startTimeOfDay;
        sunDistance = atmosphereSettings.sunDistance;
        sunSpeed = atmosphereSettings.sunSpeed;
        allowTimeFlow = atmosphereSettings.allowTimeFlow;

        sun.transform.position = new Vector3(Mathf.Cos(timeOfDay * 2 * Mathf.PI), Mathf.Sin(timeOfDay * 2 * Mathf.PI), 0) * sunDistance;
        sun.transform.LookAt(new Vector3(0, 0, 0));
    }

    private void FixedUpdate()
    {
        if (Application.isPlaying && allowTimeFlow)
        {
            // Approximately 24 mins per round if the sun speed is 1
            timeOfDay += 0.0007f * Time.fixedDeltaTime * sunSpeed;
            if (timeOfDay >= 1)
            {
                timeOfDay = 0;
            }
        }
    }
}
