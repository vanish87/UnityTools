namespace UnityTools.Common
{
	public interface IInitialize
	{
		bool Inited { get; }
		void Init(params object[] parameters);
		void Deinit(params object[] parameters);
	}
}