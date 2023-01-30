using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameInitializationAnimation : MonoBehaviour
{
    public List<string> commandsToShow = new List<string>();
    public GameObject textPrefab;
    public RectTransform commandTransform;
    public float timeBetweenLines = 0.5f;
    public float randomRange = 0.4f;

    void Start()
    {
        StartCoroutine(LoopLines());
    }

    IEnumerator LoopLines()
    {
        yield return new WaitForSeconds(0.1f);


        for(int i = 0; i < commandsToShow.Count; i++)
        {
            GameObject text = Instantiate(textPrefab, commandTransform);
            text.GetComponent<TextMeshProUGUI>().text = commandsToShow[i];

            yield return new WaitForSeconds(Random.Range(timeBetweenLines - randomRange, timeBetweenLines + randomRange));
        }

        GameObject finalText = Instantiate(textPrefab, commandTransform);
        finalText.GetComponent<TextMeshProUGUI>().text = "INITIALIZING...";

        yield return new WaitForSeconds(2f);

        DestroyImmediate(this.gameObject);
    }
}
