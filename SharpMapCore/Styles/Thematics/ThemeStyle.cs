using SharpMap.Providers;
using System;

namespace SharpMap.Styles.Thematics
{
    public class ThemeStyle : Style, IThemeStyle
    {
        Func<IFeature, IStyle> method;

        public ThemeStyle(Func<IFeature, IStyle> method)
        {
            this.method = method;
        }

        public IStyle GetStyle(IFeature attribute)
        {
            return method(attribute);
        }
    }
}
