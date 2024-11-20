//------------------------------------------------------------------------------
// <author>Zhuo YuHan</author>
// <email>1719700768@qq.com</email>
// <date>2024/11/20 14:17:04</date>
//------------------------------------------------------------------------------

using System;

namespace CodeGenerator.Core
{
    public class ResultMessage
    {
        public Boolean Success { get; set; }

        public String Msg { get; set; } = string.Empty;

        public Int32 Code { get; set; }

        public static ResultMessage Successful(String msg)
        {
            var result = new ResultMessage();
            result.Success = true;
            result.Code = 1;
            result.Msg = msg;

            return result;
        }

        public static ResultMessage Fail(String msg)
        {
            var result = new ResultMessage();
            result.Success = false;
            result.Code = 2;
            result.Msg = msg;

            return result;
        }
    }

    public class ResultMessage<T> : ResultMessage
    {
        public T Data { get; set; }

        public static ResultMessage<T> Successful(String msg, T data, Int32 code)
        {
            var result = new ResultMessage<T>();
            result.Success = true;
            result.Code = code;
            result.Msg = msg;
            result.Data = data;

            return result;
        }

        public static ResultMessage<T> Successful(String msg, T data)
        {
            return Successful(msg, data, 1);
        }

        public static ResultMessage<T> Fail(String msg, T data, Int32 code)
        {
            var result = new ResultMessage<T>();
            result.Success = false;
            result.Code = code;
            result.Msg = msg;
            result.Data = data;

            return result;
        }

        public static ResultMessage<T> Fail(String msg, T data)
        {
            var result = new ResultMessage<T>();
            result.Success = false;
            result.Code = 2;
            result.Msg = msg;
            result.Data = data;

            if (typeof(T) == typeof(UInt16))
            {
                var code = Convert.ToUInt16(data);
                if (code > 0)
                {
                    result.Code = code;
                }
            }

            if (typeof(T) == typeof(Byte))
            {
                var code = Convert.ToByte(data);
                if (code > 0)
                {
                    result.Code = code;
                }
            }

            return result;
        }

        public static ResultMessage<ushort> Fail(string v, object p)
        {
            throw new NotImplementedException();
        }
    }
}
