// using NUnit.Framework;
// using UnityEngine;
// using System.IO;

// public class SaveControllerTests
// {
//     private SaveController saveController;
//     private string saveLocation;

//     [SetUp]
//     public void SetUp()
//     {
//         saveController = new GameObject().AddComponent<SaveController>();
//         saveController.Start();

//         saveLocation = Path.Combine(Application.persistentDataPath, "Saves");
//         Directory.CreateDirectory(saveLocation); // ensure save directory exists
//     }

//     [Test]
//     public void CreateSaveTest()
//     {
//         int slot = 1;
//         string saveName = "TestSave";
//         saveController.CreateSave(slot, saveName);

//         string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
//         Assert.IsTrue(File.Exists(savePath), "Save file should be created.");

//         // verify contents of save file
//         SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
//         Assert.AreEqual(saveName, saveData.saveName, "Save name should match.");
//         Assert.AreEqual(new Vector3(-5.5f, 1.25f, 0f), saveData.playerPosition, "Player position should be default.");
//         Assert.AreEqual("Ihi_Room", saveData.mapBoundary, "Map boundary should be default.");
//         Assert.AreEqual(1, saveData.currentDay, "Current day should be default.");
//         Assert.IsFalse(saveData.isNight, "isNight should be false by default.");
//     }

//     [TearDown]
//     public void TearDown()
//     {
//         string savePath = Path.Combine(saveLocation, "save_1.json");
//         if (File.Exists(savePath))
//         {
//             File.Delete(savePath);
//         }
//         Object.Destroy(saveController.gameObject);
//     }
// }