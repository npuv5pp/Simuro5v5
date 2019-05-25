using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Simuro5v5;
using Simuro5v5.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tests
{
    public class SquareTest
    {
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator DemoTest()
        {
            SceneManager.LoadScene("GameScene_Play");
            Debug.Log("Load done.");
            for (int i = 0; i < 10; ++i)
            {
                yield return null; // new WaitForSeconds(1);
            }
        }
    }
}
