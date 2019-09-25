using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityTools;
using UnityTools.Common;

namespace Tests
{
    public class NewTestScript
    {
        [SetUp]
        public void Setup()
        {

        }

        [TearDown]
        public void TearDown()
        {

        }
        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator NewTestScriptWithEnumeratorPasses()
        {
            var go = new GameObject("test");
            go.FindOrAddTypeInComponentsAndChilden<Camera>();
            var obj = new DisposableMaterial(Shader.Find("UnityTools/RenderToDepth"));

            obj.Dispose();

            yield return new WaitForEndOfFrame();

            Assert.IsNull(obj.Data);
            Assert.IsNotNull(obj);
            
            Assert.DoesNotThrow(()=> { go.GetComponent<Camera>(); });
            Assert.Throws<MissingComponentException>(() => { throw new MissingComponentException(); });
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
