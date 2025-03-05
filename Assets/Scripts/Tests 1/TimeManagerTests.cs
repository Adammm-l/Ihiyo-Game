/* using System.Collections;
using UnityEngine;
using TMPro;

namespace Tests
{
    public class TimeManagerTests : MonoBehaviour
    {
        private GameObject gameObject;
        private TimeManager timeManager;
        private TextMeshProUGUI mockTimeDisplay;

        [RuntimeInitializeOnLoadMethod]
        public static void RunTests()
        {
            GameObject testRunner = new GameObject("TestRunner");
            testRunner.AddComponent<TimeManagerTests>();
            DontDestroyOnLoad(testRunner);
        }

        void Start()
        {
            StartCoroutine(RunAllTests());
        }

        IEnumerator RunAllTests()
        {
            Debug.Log("==== Starting TimeManager Test ====");

            yield return StartCoroutine(SetupTest());
            yield return StartCoroutine(TestMinuteIncrement());

            Debug.Log("==== TimeManager Test Complete ====");

            // Clean up
            Destroy(gameObject);
            Destroy(mockTimeDisplay.gameObject);
            Destroy(gameObject);
        }

        IEnumerator SetupTest()
        {
            gameObject = new GameObject();
            timeManager = gameObject.AddComponent<TimeManager>();

            GameObject textObject = new GameObject();
            mockTimeDisplay = textObject.AddComponent<TextMeshProUGUI>();
            var timeDisplayField = typeof(TimeManager).GetField("timeDisplay",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timeDisplayField.SetValue(timeManager, mockTimeDisplay);

            var secondsPerMinuteField = typeof(TimeManager).GetField("secondsPerMinute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            secondsPerMinuteField.SetValue(timeManager, 0.1f); 

            yield return null;
        }

        IEnumerator TestMinuteIncrement()
        {
            Debug.Log("Testing minute increment...");

            timeManager.SendMessage("Start");

            //Wait for time to increment
            yield return new WaitForSeconds(0.15f);

            //Check if minute increased
            bool passed = timeManager.GetMinute() == 1 && timeManager.GetHour() == 8;

            if (passed)
                Debug.Log("PASS: Minute Increment works");
            else
                Debug.LogError("FAIL: Minutes are not Incrementing");

            yield return null;
        }
    }
}

*/