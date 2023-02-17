using UnityEngine;
using TMPro;

public class FPSDisplay : MonoBehaviour
{
	public TextMeshProUGUI FpsText;

	private float pollingTime = 0.3f;
	private float time;
	private int frameCount;


	void Update()
	{
		// Update time.
		time += Time.deltaTime;

		// Count this frame.
		frameCount++;

		if (time >= pollingTime)
		{
			// Update frame rate.
			int frameRate = Mathf.RoundToInt((float)frameCount / time);
			FpsText.text =  "fps: " + frameRate.ToString();

			// Reset time and frame count.
			time -= pollingTime;
			frameCount = 0;
		}
	}
}