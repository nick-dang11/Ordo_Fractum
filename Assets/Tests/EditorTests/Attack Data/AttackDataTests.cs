using System;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public class AttackDataTests
{
    private const string SampleAttackAssetName =
        "SO_LongS_OxGuardR_AttackThrust_OxGuardR";

    private Type attackDataType;

    private FieldInfo animationStateNameField;
    private FieldInfo startHitFrameField;
    private FieldInfo endHitFrameField;
    private FieldInfo totalFramesField;

    private PropertyInfo startNormalizedProperty;
    private PropertyInfo endNormalizedProperty;

    private MethodInfo isAttackValidMethod;

    [SetUp]
    public void SetUp()
    {
        attackDataType =
            Type.GetType(
                "AttackData, Assembly-CSharp"
            );

        Assert.IsNotNull(
            attackDataType,
            "Could not find AttackData in Assembly-CSharp."
        );

        animationStateNameField =
            attackDataType.GetField(
                "animationStateName",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        startHitFrameField =
            attackDataType.GetField(
                "startHitFrame",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        endHitFrameField =
            attackDataType.GetField(
                "endHitFrame",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        totalFramesField =
            attackDataType.GetField(
                "totalFrames",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        startNormalizedProperty =
            attackDataType.GetProperty(
                "StartNormalized",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        endNormalizedProperty =
            attackDataType.GetProperty(
                "EndNormalized",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        isAttackValidMethod =
            attackDataType.GetMethod(
                "IsAttackValid",
                BindingFlags.Instance |
                BindingFlags.Public
            );

        Assert.IsNotNull(animationStateNameField);
        Assert.IsNotNull(startHitFrameField);
        Assert.IsNotNull(endHitFrameField);
        Assert.IsNotNull(totalFramesField);

        Assert.IsNotNull(startNormalizedProperty);
        Assert.IsNotNull(endNormalizedProperty);

        Assert.IsNotNull(
            isAttackValidMethod,
            "Could not find public AttackData.IsAttackValid()."
        );
    }

    // =========================================================
    // NORMALIZED TIMING
    // =========================================================

    [Test]
    public void StartNormalized_ConvertsFrameToNormalizedTime()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 32,
                endFrame: 45,
                totalFrames: 76
            );

        try
        {
            Assert.AreEqual(
                32f / 76f,
                GetStartNormalized(attackData),
                0.0001f,
                "StartNormalized should equal startHitFrame / totalFrames."
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void EndNormalized_ConvertsFrameToNormalizedTime()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 32,
                endFrame: 45,
                totalFrames: 76
            );

        try
        {
            Assert.AreEqual(
                45f / 76f,
                GetEndNormalized(attackData),
                0.0001f,
                "EndNormalized should equal endHitFrame / totalFrames."
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void NormalizedTiming_RemainsDeterministicAcrossRepeatedReads()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 32,
                endFrame: 45,
                totalFrames: 76
            );

        try
        {
            float expectedStart =
                GetStartNormalized(
                    attackData
                );

            float expectedEnd =
                GetEndNormalized(
                    attackData
                );

            for (int i = 0; i < 10; i++)
            {
                Assert.AreEqual(
                    expectedStart,
                    GetStartNormalized(attackData),
                    0.0001f,
                    "Repeated reads should not alter StartNormalized."
                );

                Assert.AreEqual(
                    expectedEnd,
                    GetEndNormalized(attackData),
                    0.0001f,
                    "Repeated reads should not alter EndNormalized."
                );
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void ZeroTotalFrames_ReturnsSafeNormalizedFallbacks()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 10,
                endFrame: 20,
                totalFrames: 0
            );

        try
        {
            Assert.AreEqual(
                0f,
                GetStartNormalized(attackData),
                0.0001f
            );

            Assert.AreEqual(
                1f,
                GetEndNormalized(attackData),
                0.0001f
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void FullClipWindow_MapsFromZeroToOne()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 0,
                endFrame: 76,
                totalFrames: 76
            );

        try
        {
            Assert.AreEqual(
                0f,
                GetStartNormalized(attackData),
                0.0001f
            );

            Assert.AreEqual(
                1f,
                GetEndNormalized(attackData),
                0.0001f
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    // =========================================================
    // VALIDATION API
    // =========================================================

    [Test]
    public void IsAttackValid_ValidWindow_ReturnsTrue()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 32,
                endFrame: 45,
                totalFrames: 76
            );

        try
        {
            Assert.IsTrue(
                IsAttackValid(attackData),
                "A legal authored hit window should pass validation."
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void IsAttackValid_NegativeStartFrame_ReturnsFalse()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: -1,
                endFrame: 45,
                totalFrames: 76
            );

        try
        {
            ExpectInvalidFrameDataLog();

            Assert.IsFalse(
                IsAttackValid(attackData)
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void IsAttackValid_NegativeEndFrame_ReturnsFalse()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 0,
                endFrame: -1,
                totalFrames: 76
            );

        try
        {
            ExpectInvalidFrameDataLog();

            Assert.IsFalse(
                IsAttackValid(attackData)
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void IsAttackValid_ZeroTotalFrames_ReturnsFalse()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 0,
                endFrame: 10,
                totalFrames: 0
            );

        try
        {
            ExpectInvalidFrameDataLog();

            Assert.IsFalse(
                IsAttackValid(attackData)
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void IsAttackValid_StartAfterEnd_ReturnsFalse()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 46,
                endFrame: 45,
                totalFrames: 76
            );

        try
        {
            ExpectStartGreaterThanOrEqualToEndLog();

            Assert.IsFalse(
                IsAttackValid(attackData)
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void IsAttackValid_StartEqualsEnd_ReturnsFalse()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 45,
                endFrame: 45,
                totalFrames: 76
            );

        try
        {
            ExpectStartGreaterThanOrEqualToEndLog();

            Assert.IsFalse(
                IsAttackValid(attackData),
                "A zero-length hit window should be invalid."
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    [Test]
    public void IsAttackValid_EndBeyondTotalFrames_ReturnsFalse()
    {
        ScriptableObject attackData =
            CreateAttackData(
                startFrame: 32,
                endFrame: 77,
                totalFrames: 76
            );

        try
        {
            LogAssert.Expect(
                LogType.Error,
                new Regex(
                    "endHitFrame .* exceeds totalFrames"
                )
            );

            Assert.IsFalse(
                IsAttackValid(attackData)
            );
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(
                attackData
            );
        }
    }

    // =========================================================
    // REAL AUTHORED ASSET REGRESSION
    // =========================================================

    [Test]
    public void SampleOxGuardWindow_IsValid()
    {
        UnityEngine.Object sampleAsset =
            LoadSampleAttackAsset();

        Assert.IsTrue(
            IsAttackValid(sampleAsset),
            "The authored Ox Guard thrust asset should remain valid."
        );
    }

    [Test]
    public void SampleOxGuardWindow_MatchesAuthoredRegressionValues()
    {
        UnityEngine.Object sampleAsset =
            LoadSampleAttackAsset();

        Assert.AreEqual(
            32,
            GetIntField(
                sampleAsset,
                startHitFrameField
            )
        );

        Assert.AreEqual(
            45,
            GetIntField(
                sampleAsset,
                endHitFrameField
            )
        );

        Assert.AreEqual(
            76,
            GetIntField(
                sampleAsset,
                totalFramesField
            )
        );

        Assert.AreEqual(
            32f / 76f,
            GetStartNormalized(sampleAsset),
            0.0001f
        );

        Assert.AreEqual(
            45f / 76f,
            GetEndNormalized(sampleAsset),
            0.0001f
        );
    }

    [Test]
    public void SampleOxGuardAsset_HasAnimationStateName()
    {
        UnityEngine.Object sampleAsset =
            LoadSampleAttackAsset();

        string animationStateName =
            animationStateNameField.GetValue(
                sampleAsset
            ) as string;

        Assert.IsFalse(
            string.IsNullOrWhiteSpace(
                animationStateName
            ),
            "The sample AttackData asset should identify its Animator state."
        );
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private ScriptableObject CreateAttackData(
        int startFrame,
        int endFrame,
        int totalFrames)
    {
        ScriptableObject attackData =
            ScriptableObject.CreateInstance(
                attackDataType
            );

        startHitFrameField.SetValue(
            attackData,
            startFrame
        );

        endHitFrameField.SetValue(
            attackData,
            endFrame
        );

        totalFramesField.SetValue(
            attackData,
            totalFrames
        );

        return attackData;
    }

    private UnityEngine.Object LoadSampleAttackAsset()
    {
        string[] guids =
            AssetDatabase.FindAssets(
                SampleAttackAssetName +
                " t:AttackData"
            );

        Assert.AreEqual(
            1,
            guids.Length,
            $"Expected exactly one {SampleAttackAssetName} AttackData asset, " +
            $"but found {guids.Length}."
        );

        string path =
            AssetDatabase.GUIDToAssetPath(
                guids[0]
            );

        UnityEngine.Object sampleAsset =
            AssetDatabase.LoadAssetAtPath(
                path,
                attackDataType
            );

        Assert.IsNotNull(
            sampleAsset,
            $"Could not load AttackData asset at '{path}'."
        );

        return sampleAsset;
    }

    private bool IsAttackValid(
        UnityEngine.Object attackData)
    {
        return
            (bool)isAttackValidMethod.Invoke(
                attackData,
                null
            );
    }

    private float GetStartNormalized(
        UnityEngine.Object attackData)
    {
        return
            (float)startNormalizedProperty.GetValue(
                attackData
            );
    }

    private float GetEndNormalized(
        UnityEngine.Object attackData)
    {
        return
            (float)endNormalizedProperty.GetValue(
                attackData
            );
    }

    private static int GetIntField(
        UnityEngine.Object target,
        FieldInfo field)
    {
        return
            (int)field.GetValue(
                target
            );
    }

    private static void ExpectInvalidFrameDataLog()
    {
        LogAssert.Expect(
            LogType.Error,
            new Regex(
                "Invalid frame data in .*startHitFrame=.*endHitFrame=.*totalFrames=.*"
            )
        );
    }

    private static void ExpectStartGreaterThanOrEqualToEndLog()
    {
        LogAssert.Expect(
            LogType.Error,
            new Regex(
                "startHitFrame .* is greater than OR EQUAL TO endHitFrame"
            )
        );
    }
}
