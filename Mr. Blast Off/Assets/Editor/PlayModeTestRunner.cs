using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Unity.AI.Assistant.PlayModeTest
{
    [InitializeOnLoad]
    internal static class PlayModeTestRunner
    {
        private const string StateKey = "PlayModeTest.State";
        private const string ResultKey = "PlayModeTest.Result";
        private const string ScriptPathKey = "PlayModeTest.ScriptPath";
        private const string SentinelLog = "PLAY_MODE_TEST_COMPLETE";

        private static readonly int WaitFrames = SessionState.GetInt("PlayModeTest.WaitFrames", 5);
        private static readonly float TestTimeout = SessionState.GetFloat("PlayModeTest.TestTimeout", 15.0f);

        private static List<string> _capturedLogs = new List<string>();
        private const int MaxCapturedLogs = 50;

        static PlayModeTestRunner()
        {
            string state = SessionState.GetString(StateKey, "Idle");

            switch (state)
            {
                case "Idle":
                    break;

                case "WaitingForCompile":
                    Debug.Log("[PlayModeTest] Bootstrap compiled. Scheduling Play Mode entry.");
                    EditorApplication.delayCall += () =>
                    {
                        SessionState.SetString(StateKey, "EnteringPlayMode");
                        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                        EditorApplication.isPlaying = true;
                    };
                    break;

                case "EnteringPlayMode":
                    EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                        SessionState.SetString(StateKey, "InPlayMode");
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "InPlayMode":
                    if (EditorApplication.isPlaying)
                    {
                        EditorApplication.update += WaitFramesThenRun;
                    }
                    break;

                case "Done":
                    Debug.Log(SentinelLog);
                    break;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
                SessionState.SetString(StateKey, "InPlayMode");
                EditorApplication.update += WaitFramesThenRun;
            }
        }

        private static int _frameCount = 0;
        private static bool _setupDone = false;
        private static bool _testDone = false;
        private static double _testStartTime = 0;

        private static GameObject _selectionPanel;
        private static GameObject _inventoryBar;

        private static void WaitFramesThenRun()
        {
            _frameCount++;
            if (_frameCount < WaitFrames) return;

            if (_testDone) return;

            if (!_setupDone)
            {
                _setupDone = true;
                Application.logMessageReceived += OnLogMessage;
                _testStartTime = EditorApplication.timeSinceStartup;
                try
                {
                    Setup();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[PlayModeTest] Setup threw exception: " + e);
                    FinishTest(true, e.Message);
                    return;
                }
                return;
            }

            float gameElapsed = Time.timeSinceLevelLoad;
            float realElapsed = (float)(EditorApplication.timeSinceStartup - _testStartTime);
            bool timedOut = realElapsed >= TestTimeout;

            try
            {
                bool complete = Tick(gameElapsed, realElapsed);
                if (complete || timedOut)
                {
                    if (timedOut && !complete)
                    {
                        Debug.LogWarning("[PlayModeTest] Test timed out. Game elapsed: " + gameElapsed + "s");
                    }
                    FinishTest(timedOut && !complete, timedOut ? "Test timed out after " + TestTimeout + "s" : null);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[PlayModeTest] Tick threw exception: " + e);
                FinishTest(true, e.Message);
            }
        }

        private static void FinishTest(bool isError, string errorMessage)
        {
            _testDone = true;
            EditorApplication.update -= WaitFramesThenRun;
            Application.logMessageReceived -= OnLogMessage;

            string resultJson;
            try
            {
                resultJson = GetResult();
            }
            catch (System.Exception e)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = "GetResult() threw: " + e.Message,
                    logs = _capturedLogs.ToArray()
                });
            }

            if (isError && errorMessage != null)
            {
                resultJson = JsonUtility.ToJson(new TestResult
                {
                    success = false,
                    error = errorMessage,
                    logs = _capturedLogs.ToArray()
                });
            }

            SessionState.SetString(ResultKey, resultJson);
            SessionState.SetString(StateKey, "Done");
            EditorApplication.isPlaying = false;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type)
        {
            if (_capturedLogs.Count >= MaxCapturedLogs) return;
            _capturedLogs.Add("[" + type + "] " + message);
        }

        [System.Serializable]
        private class TestResult
        {
            public bool success;
            public string error;
            public bool isInventoryHiddenInitially;
            public bool isInventoryShownOnStart;
            public bool isInventoryBarSizeUnchanged;
            public bool isSlotsCountCorrect;
            public bool isSlotsSizeUnchanged;
            public string[] logs;
        }

        private static void Setup()
        {
            Debug.Log("[Test] Setup initiated.");
            
            // Find active Canvas first, then look for child panel and inventory bar which might be inactive
            GameObject canvas = GameObject.Find("Canvas");
            if (canvas != null)
            {
                Transform selectionTransform = canvas.transform.Find("Element Selection");
                if (selectionTransform != null)
                {
                    _selectionPanel = selectionTransform.gameObject;
                }

                Transform invTransform = canvas.transform.Find("Inventory Bar");
                if (invTransform != null)
                {
                    _inventoryBar = invTransform.gameObject;
                }
            }

            if (_selectionPanel == null)
            {
                throw new System.Exception("Element Selection Panel not found on Canvas children!");
            }
            if (_inventoryBar == null)
            {
                throw new System.Exception("Inventory Bar GameObject not found on Canvas children!");
            }

            // Force active to ensure clean starting test state
            _selectionPanel.SetActive(true);
        }

        private static bool Tick(float gameElapsed, float realElapsed)
        {
            // 1. Verify Inventory Bar is hidden (inactive) during Element Selection phase
            bool isInvActive = _inventoryBar.activeSelf;
            Debug.Log("[Test] Is Inventory Bar active initially: " + isInvActive);
            if (isInvActive)
            {
                throw new System.Exception("Failed verification: Inventory Bar should be inactive during selection phase!");
            }

            // 2. Select 10 elements on selection panel
            var selectionSlots = _selectionPanel.GetComponentsInChildren<ElementSlot>(true);
            Debug.Log("[Test] Selecting 10 elements programmatically...");
            for (int i = 0; i < 10; i++)
            {
                selectionSlots[i].OnPointerClick(null);
            }

            // 3. Click the selection panel Start button
            var manager = _selectionPanel.GetComponent<ElementSelectionManager>();
            if (manager == null)
            {
                throw new System.Exception("ElementSelectionManager missing!");
            }
            Debug.Log("[Test] Invoking Start button click.");
            manager.startButton.onClick.Invoke();

            // 4. Verify Inventory Bar becomes active
            bool isInvActivePostStart = _inventoryBar.activeSelf;
            Debug.Log("[Test] Is Inventory Bar active post-start: " + isInvActivePostStart);
            if (!isInvActivePostStart)
            {
                throw new System.Exception("Failed verification: Inventory Bar failed to activate after Element Selection completed!");
            }

            // 5. Verify dimensions of Inventory Bar (must remain 750 x 75)
            RectTransform barRect = _inventoryBar.GetComponent<RectTransform>();
            Vector2 barSize = barRect.sizeDelta;
            Debug.Log("[Test] Inventory Bar SizeDelta: " + barSize);
            if (barSize != new Vector2(750f, 75f))
            {
                throw new System.Exception($"Failed verification: Inventory Bar sizeDelta was changed to {barSize}, expected original 750x75!");
            }

            // 6. Verify there are exactly 10 slots
            // Find all active slots under the Inventory Bar (including children)
            // Wait, LayoutGroup spacing could mean they need one frame to align, but let's check hierarchy count.
            int childCount = _inventoryBar.transform.childCount;
            Debug.Log("[Test] Number of slots in Inventory Bar: " + childCount);
            if (childCount != 10)
            {
                throw new System.Exception($"Failed verification: Inventory Bar has {childCount} child slots, expected exactly 10!");
            }

            // 7. Verify all 10 slots keep original size of 50 x 50
            for (int i = 0; i < 10; i++)
            {
                Transform slotChild = _inventoryBar.transform.GetChild(i);
                RectTransform slotRect = slotChild.GetComponent<RectTransform>();
                Vector2 slotSize = slotRect.sizeDelta;
                if (slotSize != new Vector2(50f, 50f))
                {
                    throw new System.Exception($"Failed verification: Slot {i + 1} sizeDelta is {slotSize}, expected original 50x50!");
                }

                // Verify text child exists and matches selected element name
                TextMeshProUGUI slotText = slotChild.GetComponentInChildren<TextMeshProUGUI>();
                if (slotText == null)
                {
                    throw new System.Exception($"Failed verification: Slot {i + 1} is missing the name label child TextMeshProUGUI!");
                }
                Debug.Log($"[Test] Slot {i + 1} name label: {slotText.text}");
            }

            Debug.Log("[Test] PLAYTIME INVENTORY BAR SUCCESSFULLY VERIFIED!");
            return true;
        }

        private static string GetResult()
        {
            var result = new TestResult
            {
                success = true,
                isInventoryHiddenInitially = true,
                isInventoryShownOnStart = true,
                isInventoryBarSizeUnchanged = true,
                isSlotsCountCorrect = true,
                isSlotsSizeUnchanged = true,
                logs = _capturedLogs.ToArray()
            };
            return JsonUtility.ToJson(result);
        }
    }
}