using System;
using System.Collections;
using System.Collections.Generic;
using FOW.Script;
using GameUtils;
using GameUtils.ManagersAndSystems;
using GameUtils.Objects.Entities;
using UnityEngine;

public class FogCoverable : MonoBehaviour
{
    private Renderer renderer;
    private SpriteRenderer spriteRenderer;
    private GameEntity entScript;

    void Start()
    {
        renderer = GetComponent<Renderer>();
        entScript = GetComponent<GameEntity>();

        FieldOfView.OnTargetsVisibilityChange += FieldOfViewOnTargetsVisibilityChange;
    }

    void OnDestroy()
    {
        FieldOfView.OnTargetsVisibilityChange -= FieldOfViewOnTargetsVisibilityChange;
    }

    void FieldOfViewOnTargetsVisibilityChange(List<Transform> newTargets)
    {
        if (!ChunkManager.staticFogEnabled) return;
        if (Recolor.IsIgnored(gameObject.GetComponent<GameEntity>())
            && FieldOfView.visibleBefore.Contains(transform))
        {
            renderer.enabled = true;
            entScript.enabled = true;
        }
        else
        {
            var cont = newTargets.Contains(transform);
            renderer.enabled = cont;
            
            //Block script logic only on neutral ents
            if (!cont && !GroupUtil.IsNeutral(entScript.Group)) cont = true;
            entScript.enabled = cont;
        }
    }
}