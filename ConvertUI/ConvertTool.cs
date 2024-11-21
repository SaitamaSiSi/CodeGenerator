//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/19 17:26:09</date>
//------------------------------------------------------------------------------

using Avalonia.Data;
using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace CodeGenerator.ConvertUI
{
    public class ConvertTool : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            //var b = targetType.IsAssignableTo(typeof(string));
            //var txtinfo = new CultureInfo("en-US", false).TextInfo;
            //return txtinfo.ToTitleCase((string)value);


            if (parameter is string sTargetCase)
            {
                // 如果辅助参数是字符串类型
                if (value is bool bSourceText)
                {
                    // 如果原值是bool类型
                    switch (sTargetCase)
                    {
                        case "IsPrimaryKey":
                        case "IsAutoIncrement":
                            return bSourceText ? "是" : "否";
                    }
                }
                else if (value is int iSourceText)
                {
                    // 如果原值是int类型
                    switch (sTargetCase)
                    {
                        case "Length":
                            {
                                return iSourceText > 0 ? iSourceText.ToString() : "";
                            }
                    }
                }
                else if (value is string sSourceText)
                {
                    // 如果原值是字符串类型
                    switch (sTargetCase)
                    {
                        case "IsNullable":
                            {
                                // Y/N
                                if (string.Equals(sSourceText, "N", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(sSourceText, "No", StringComparison.OrdinalIgnoreCase))
                                {
                                    return "是";
                                }
                                else if (string.Equals(sSourceText, "Y", StringComparison.OrdinalIgnoreCase) ||
                                    string.Equals(sSourceText, "Yes", StringComparison.OrdinalIgnoreCase))
                                {
                                    return "否";
                                }
                                return sSourceText;
                            }
                    }
                }
            }

            // converter used for the wrong type
            return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
