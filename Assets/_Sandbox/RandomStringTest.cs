using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

public class RandomStringTest : MonoBehaviour
{

    private string randomLetters = "!@#$%^&*()_+=-0987654321/.,<>?;'[{}]";
    public string testString = "This is a long test string that I am using to test the information in this system. We will see how it looks whenever the system randomly sets some of the letters to be not strings.";
    public float randomCharChance = 0.5f;
    public char[] punctuation = {' ', '.',',','?','!','(',')',':',';','\''};
    void Start()
    {
        float firstTime = Time.time;
        StringBuilder newString = new StringBuilder(testString);
        Debug.Log(newString);

        for(int i = 0; i++ < testString.Length - 1;)
        {
            float chance = Random.Range(0f, 1f);
            Debug.Log(chance < randomCharChance || testString[i] == ' ');

            if(chance < randomCharChance || punctuation.Contains(testString[i]))
                continue;

            newString[i] = randomLetters[Random.Range(0,randomLetters.Length - 1)];
        }

        testString = newString.ToString();

        Debug.Log(testString);
        Debug.Log(Time.time - firstTime);
    }


    void Update()
    {
        
    }
}
