/*
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests
{
    public class PlayerStartPointTests : MonoBehaviour
    {
        //private GameObject playerChara;
        private GameObject playerChara;
        private PlayerStartPoint wavePoint; 
        
        [RuntimeInitializeOnLoadMethod]
        public static void RunTests()
        {
            GameObject testRunner2 = new GameObject("TestRunner2");
            testRunner2.AddComponent<PlayerStartPointTests>();
            DontDestroyOnLoad(testRunner2);
        }
        void Start() {
            StartCoroutine(RunAllTests());
        }

        IEnumerator RunAllTests()
        {
            Debug.Log("==== Starting PlayerStartPoint Test ====");

            yield return StartCoroutine(SetupTest());
            yield return StartCoroutine(startPos());

            Debug.Log("==== All PlayerStartPoint Tests Complete ====");

            // Clean up
            Destroy(gameObject);
            Destroy(playerChara.gameObject);
            Destroy(gameObject);
        }

        public IEnumerator SetupTest() {
            // Create a new GameObject
            wavePoint = GameObject.FindObjectOfType<PlayerStartPoint>();
            wavePoint.transform.position = new Vector3(15f,10f,0f);
            playerChara = GameObject.FindGameObjectWithTag("Player");
            yield return null;
        }

        public IEnumerator startPos() {
            wavePoint.Start(); // Call the Script's start function
            yield return null;
            Assert.AreEqual(new Vector3(15f, 10f, playerChara.transform.position.z), playerChara.transform.position); // See if the player is actually at the start position
            Debug.Log("Player position is correctly set at: " + playerChara.transform.position);

        }
    }

}

*/