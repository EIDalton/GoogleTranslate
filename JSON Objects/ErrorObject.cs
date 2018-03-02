using System;

namespace GoogleTranslateAPI.JSON
{
    public class Error
    {
        public int code { get; set; }
        public string message { get; set; }
    }

    public class ErrorRoot
    {
        public Error error { get; set; }
    }
}