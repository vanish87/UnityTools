


// Z buffer depth to linear 0-1 depth
// Handles orthographic projection correctly
float Linear01DepthWithOrtho(float z)
{
	float isOrtho = unity_OrthoParams.w;
	if(isOrtho > 0)
	{
		#if defined(UNITY_REVERSED_Z)
		z = 1 - z;//remap to [0,1]
		#endif
	}
	else
	{
		z = Linear01Depth(z);
	}

	return z;
}
