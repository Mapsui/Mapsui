using System.ComponentModel;

namespace System.Runtime.CompilerServices;

#if NETSTANDARD2_0 ||  NETCOREAPP2_0 ||  NETCOREAPP2_1 ||  NETCOREAPP2_2 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47 || NET471 || NET472 || NET48

[EditorBrowsable(EditorBrowsableState.Never)]
internal class IsExternalInit { }
#endif
