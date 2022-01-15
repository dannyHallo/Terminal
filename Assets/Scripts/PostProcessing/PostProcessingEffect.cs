﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PostProcessingEffect : ScriptableObject {

	protected Material material;

	public virtual void ReleaseBuffers () {}

	public abstract void Render (RenderTexture source, RenderTexture destination);
}