using System;
using System.Collections.Generic;
using System.Text;

namespace Architecture.Core.Model
{
    public class BaseResponse<T>
    {
        public T Data { get; set; }
        public List<T> DataList { get; set; }
        public bool Status { get; set; } = true;
        public long ID { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }

    public class BaseResponse
    {
        public long ID { get; set; }
        public bool Status { get; set; } = true;
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
