/*

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
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
            Debug.Log("==== Starting TimeManager Tests ====");

            yield return StartCoroutine(SetupTest());
            yield return StartCoroutine(TestInitialValues());
            yield return StartCoroutine(TestMinuteIncrement());
            yield return StartCoroutine(TestHourRollover());
            yield return StartCoroutine(Test24HourRollover());
            yield return StartCoroutine(TestTimeStringFormatting());

            Debug.Log("==== All TimeManager Tests Complete ====");

            // Clean up
            Destroy(gameObject);
            Destroy(mockTimeDisplay.gameObject);
            Destroy(gameObject);
        }

        IEnumerator SetupTest()
        {
            // Create GameObject with TimeManager
            gameObject = new GameObject();
            timeManager = gameObject.AddComponent<TimeManager>();

            // Create mock TextMeshProUGUI
            GameObject textObject = new GameObject();
            mockTimeDisplay = textObject.AddComponent<TextMeshProUGUI>();

            // Use reflection to set private serialized fields
            var timeDisplayField = typeof(TimeManager).GetField("timeDisplay",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            timeDisplayField.SetValue(timeManager, mockTimeDisplay);

            var secondsPerMinuteField = typeof(TimeManager).GetField("secondsPerMinute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            secondsPerMinuteField.SetValue(timeManager, 0.1f); // Fast time for testing

            yield return null;
        }

        IEnumerator TestInitialValues()
        {
            Debug.Log("Testing initial values...");

            // Call Start method manually
            timeManager.SendMessage("Start");

            // Verify initial values
            bool passed = timeManager.GetHour() == 8 &&
                          timeManager.GetMinute() == 0 &&
                          timeManager.GetTimeString() == "08:00 AM";

            if (passed)
                Debug.Log("✓ Initial values test PASSED");
            else
                Debug.LogError("✗ Initial values test FAILED");

            yield return null;
        }

        IEnumerator TestMinuteIncrement()
        {
            Debug.Log("Testing minute increment...");

            timeManager.SendMessage("Start");

            // Wait for time to increment
            yield return new WaitForSeconds(0.15f); // Slightly longer than secondsPerMinute

            // Check if minute increased
            bool passed = timeManager.GetMinute() == 1 && timeManager.GetHour() == 8;

            if (passed)
                Debug.Log("✓ Minute increment test PASSED");
            else
                Debug.LogError("✗ Minute increment test FAILED");

            yield return null;
        }

        IEnumerator TestHourRollover()
        {
            Debug.Log("Testing hour rollover...");

            timeManager.SendMessage("Start");

            // Use reflection to set minutes to 59
            var gameMinuteField = typeof(TimeManager).GetField("gameMinute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gameMinuteField.SetValue(timeManager, 59);

            // Wait for time to increment
            yield return new WaitForSeconds(0.15f);

            // Check if hour increased and minutes reset
            bool passed = timeManager.GetMinute() == 0 && timeManager.GetHour() == 9;

            if (passed)
                Debug.Log("✓ Hour rollover test PASSED");
            else
                Debug.LogError("✗ Hour rollover test FAILED");

            yield return null;
        }

        IEnumerator Test24HourRollover()
        {
            Debug.Log("Testing 24-hour rollover...");

            timeManager.SendMessage("Start");

            // Use reflection to set hour to 23 and minute to 59
            var gameHourField = typeof(TimeManager).GetField("gameHour",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gameHourField.SetValue(timeManager, 23);

            var gameMinuteField = typeof(TimeManager).GetField("gameMinute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gameMinuteField.SetValue(timeManager, 59);

            // Wait for time to increment
            yield return new WaitForSeconds(0.15f);

            // Check if hour and minute reset
            bool passed = timeManager.GetMinute() == 0 && timeManager.GetHour() == 0;

            if (passed)
                Debug.Log("✓ 24-hour rollover test PASSED");
            else
                Debug.LogError("✗ 24-hour rollover test FAILED");

            yield return null;
        }

        IEnumerator TestTimeStringFormatting()
        {
            Debug.Log("Testing time string formatting...");

            timeManager.SendMessage("Start");
            bool allPassed = true;

            // Test AM time
            if (timeManager.GetTimeString() != "08:00 AM")
            {
                Debug.LogError("✗ AM time formatting test FAILED");
                allPassed = false;
            }

            // Test PM time
            var gameHourField = typeof(TimeManager).GetField("gameHour",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gameHourField.SetValue(timeManager, 15); // 3 PM

            var gameMinuteField = typeof(TimeManager).GetField("gameMinute",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            gameMinuteField.SetValue(timeManager, 30);

            if (timeManager.GetTimeString() != "03:30 PM")
            {
                Debug.LogError("✗ PM time formatting test FAILED");
                allPassed = false;
            }

            // Test noon (12 PM)
            gameHourField.SetValue(timeManager, 12);
            gameMinuteField.SetValue(timeManager, 0);

            if (timeManager.GetTimeString() != "12:00 PM")
            {
                Debug.LogError("✗ Noon formatting test FAILED");
                allPassed = false;
            }

            // Test midnight (12 AM)
            gameHourField.SetValue(timeManager, 0);
            gameMinuteField.SetValue(timeManager, 0);

            if (timeManager.GetTimeString() != "12:00 AM")
            {
                Debug.LogError("✗ Midnight formatting test FAILED");
                allPassed = false;
            }

            if (allPassed)
                Debug.Log("✓ Time string formatting tests PASSED");

            yield return null;
        }
    }
}

*/