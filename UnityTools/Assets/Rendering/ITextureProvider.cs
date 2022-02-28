
using UnityEngine;

namespace UnityTools.Rendering
{
	public interface ITextureProvider<T> : TypeValue<T>, ITextureProvider where T : System.Enum
	{
	}

	public interface ITextureProvider
	{
		Texture Tex { get; }
	}
}
