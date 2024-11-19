//------------------------------------------------------------------------------
// <copyright file="ColumnParam.cs" company="CQ ULIT Co., Ltd.">
//    Copyright (c) 2024, Chongqing Youliang Science & Technology Co., Ltd. All rights reserved.
// </copyright>
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/15 16:49:06</date>
//------------------------------------------------------------------------------

namespace CodeGenerator.Model
{
    public class ColumnParam
    {
        public string Name { get; set; } = string.Empty;
        public string Comments { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// N/Y
        /// </summary>
        public string IsNullable { get; set; } = string.Empty;
    }
}
