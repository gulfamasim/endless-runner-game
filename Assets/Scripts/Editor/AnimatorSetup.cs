using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;

public class AnimatorSetup : EditorWindow
{
    [MenuItem("Tools/Neon Rift/Setup Character Animator")]
    static void Setup()
    {
        var found = new Dictionary<string, AnimationClip>();
        foreach (var guid in AssetDatabase.FindAssets("t:Object"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (!path.ToLower().EndsWith(".fbx")) continue;
            string file = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is AnimationClip c && !c.name.StartsWith("__preview__"))
                    if (!found.ContainsKey(file)) { found[file] = c; Debug.Log($"[Anim] {file} => {c.name}"); }
            }
        }

        AnimationClip Pick(params string[] keys)
        {
            foreach (var k in keys)
                foreach (var kv in found)
                    if (kv.Key.Contains(k.ToLower())) return kv.Value;
            return null;
        }

        var clipRun   = Pick("fast_run","run","ch46");
        var clipJump  = Pick("jump");
        var clipSlide = Pick("slide");
        var clipTurnL = Pick("left_turn","left");
        var clipTurnR = Pick("right_turn","right");
        var clipDie   = Pick("flying_back","death","die","flying");

        const string P = "Assets/NeonRiftAnimator.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(P) != null)
            AssetDatabase.DeleteAsset(P);
        var ac = AnimatorController.CreateAnimatorControllerAtPath(P);

        ac.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
        ac.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
        ac.AddParameter("IsSliding", AnimatorControllerParameterType.Bool);
        ac.AddParameter("TurnLeft",  AnimatorControllerParameterType.Trigger);
        ac.AddParameter("TurnRight", AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Die",       AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Revive",    AnimatorControllerParameterType.Trigger);

        var sm = ac.layers[0].stateMachine;

        var sRun   = sm.AddState("Run");
        var sJump  = sm.AddState("Jump");
        var sSlide = sm.AddState("Slide");
        var sTurnL = sm.AddState("TurnLeft");
        var sTurnR = sm.AddState("TurnRight");
        var sDie   = sm.AddState("Die");

        sm.defaultState = sRun;  // start in Run — no idle needed

        if (clipRun   != null) sRun.motion   = clipRun;
        if (clipJump  != null) sJump.motion  = clipJump;
        if (clipSlide != null) sSlide.motion = clipSlide;
        if (clipTurnL != null) sTurnL.motion = clipTurnL;
        if (clipTurnR != null) sTurnR.motion = clipTurnR;
        if (clipDie   != null) { sDie.motion = clipDie; sDie.speed = 1f; }

        // Helper
        AnimatorStateTransition Tr(AnimatorState from, AnimatorState to,
            float dur=0.12f, bool exit=false, float exitT=0f)
        {
            var t = from.AddTransition(to);
            t.hasExitTime=exit; t.exitTime=exitT; t.duration=dur; return t;
        }

        // Run <-> Jump
        Tr(sRun,  sJump).AddCondition(AnimatorConditionMode.If,    0, "IsJumping");
        Tr(sJump, sRun ).AddCondition(AnimatorConditionMode.IfNot, 0, "IsJumping");

        // Run <-> Slide
        Tr(sRun,   sSlide).AddCondition(AnimatorConditionMode.If,    0, "IsSliding");
        Tr(sSlide, sRun  ).AddCondition(AnimatorConditionMode.IfNot, 0, "IsSliding");

        // Run -> TurnL -> back to Run
        Tr(sRun, sTurnL).AddCondition(AnimatorConditionMode.If, 0, "TurnLeft");
        Tr(sTurnL, sRun, 0.1f, true, 0.6f);

        // Run -> TurnR -> back to Run
        Tr(sRun, sTurnR).AddCondition(AnimatorConditionMode.If, 0, "TurnRight");
        Tr(sTurnR, sRun, 0.1f, true, 0.6f);

        // ANY -> Die (interrupt everything)
        var ad = sm.AddAnyStateTransition(sDie);
        ad.AddCondition(AnimatorConditionMode.If, 0, "Die");
        ad.hasExitTime = false; ad.duration = 0.1f; ad.canTransitionToSelf = false;

        // Die -> Run after Revive trigger
        Tr(sDie, sRun, 0.25f).AddCondition(AnimatorConditionMode.If, 0, "Revive");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Assign to player
        var pc = FindFirstObjectByType<PlayerController>();
        if (pc != null)
        {
            var anim = pc.GetComponentInChildren<Animator>();
            if (anim == null)
            {
                var smr = pc.GetComponentInChildren<SkinnedMeshRenderer>();
                anim = smr != null
                    ? smr.gameObject.AddComponent<Animator>()
                    : pc.gameObject.AddComponent<Animator>();
            }
            anim.runtimeAnimatorController = ac;
            anim.applyRootMotion = false;
            EditorUtility.SetDirty(pc.gameObject);
            Debug.Log("[Anim] Controller assigned!");
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        var clips = new[]{("Run",clipRun),("Jump",clipJump),("Slide",clipSlide),
                          ("TurnL",clipTurnL),("TurnR",clipTurnR),("Die",clipDie)};
        string msg = string.Join("\n", clips.Select(x =>
            x.Item2 != null ? $"✅ {x.Item1}" : $"❌ {x.Item1} — NOT FOUND"));
        int miss = clips.Count(x => x.Item2 == null);
        msg += miss == 0 ? "\n\n🎉 All clips found! Press Play." :
               $"\n\n⚠ {miss} missing. Check FBX files are in Assets/ folder.";
        EditorUtility.DisplayDialog("Animator Setup", msg, "OK");
    }
}
